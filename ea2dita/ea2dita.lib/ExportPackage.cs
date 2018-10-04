using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EA;
using File = System.IO.File;
using System.Windows.Forms;

namespace ea2dita.lib
{
    public class ExportPackage
    {
        private const int max_path_length = 240;

        private Repository repository;
        private string root_path;
        private string topics_root;
        private Project project;
        private ExportOptions options;

        private ExportConfig config = new ExportConfig();

        public ExportPackage(Repository repository, string root_path, Package package, ExportOptions options)
        {
            string prefix = string.Empty;

            var parent = package.ParentID;
            while (parent != 0 && parent != package.PackageID)
            {
                var parentPackage = repository.GetPackageByID(parent);
                if (0 == parentPackage.ParentID)
                {
                    break;
                }
                var name = GetPackageFolderName(parentPackage);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (string.IsNullOrWhiteSpace(prefix))
                    {
                        prefix = name;
                    }
                    else
                    {
                        prefix = Path.Combine(name, prefix);
                    }
                }

                parent = parentPackage.ParentID;
            }

            this.topics_root = Path.Combine("topics", "model", prefix);
            this.repository = repository;
            this.project = repository.GetProjectInterface();
            this.root_path = Path.Combine(root_path, this.topics_root); ;
            this.options = options;
        }

        public static void Export(Repository repository, Package package, string outputFile, ExportOptions options)
        {
            var export = new ExportPackage(
                repository,
                Path.GetDirectoryName(outputFile),
                package,
                options);
            export.Export(package, outputFile);
        }

        private string GetPackageFolderName(Package package)
        {
            return ExportConfig.Translit(this.config.FirstNotEmpty(package.Alias, package.Name, package.PackageID));
        }

        private void Export(Package package, string outputFile)
        {
            if (outputFile.Length > max_path_length)
            {
                MessageBox.Show($"Very long name {outputFile}");
                return;
            }

            using (var writer = XmlWriter.Create(outputFile, new XmlWriterSettings
            {
                Indent = true
            }))
            {
                writer.WriteStartDocument();
                
                writer.WriteDocType("map", "-//OASIS//DTD DITA Map//EN", "map.dtd", null);

                writer.WriteStartElement("map");
                writer.WriteAttributeString("xml", "lang", null, "ru");

                writer.WriteElementString("title", package.Name);


                ExportObject(package, GetPackageFolderName(package), writer, true);

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        void ExportObject(object obj, string path, XmlWriter tocWriter, bool is_top)
        {
            var folder_name = Path.Combine(root_path, Path.GetDirectoryName(path));
            if (folder_name.Length > max_path_length)
            {
                MessageBox.Show($"Very long name {folder_name}");
                return;
            }

            Directory.CreateDirectory(folder_name);
            
            var topic_config = this.config.GetTopicConfig(obj);
            if (null != topic_config)
            {
                if (!topic_config.diagrams_func(obj).IsEmpty())
                {
                    Directory.CreateDirectory(Path.Combine(folder_name, "images"));

                    foreach (Diagram diagram in topic_config.diagrams_func(obj))
                    {
                        repository.OpenDiagram(diagram.DiagramID);
                        project.SaveDiagramImageToFile(
                            Path.Combine(
                                root_path,
                                Path.GetDirectoryName(path),
                                "images",
                                ExportConfig.Translit(diagram.Name) + ".png"));
                        repository.CloseDiagram(diagram.DiagramID);
                    }
                }

                ExportObjectBody(obj, topic_config, path, tocWriter);
                tocWriter.WriteStartElement("topicref");
                tocWriter.WriteAttributeString("href", Path.Combine(topics_root, path).Replace('\\', '/') + ".dita");
                if (is_top)
                {
                    tocWriter.WriteEndElement(); //topicref
                }

                var children_config = this.config.GetChildrenConfig(obj);
                if (null != children_config)
                {
                    foreach (var child_config in children_config)
                    {
                        foreach (var child in child_config(obj))
                        {
                            if (this.options.HideEmptyElements && IsObjectEmpty(child) || this.config.IsSimple(child))
                            {
                                continue;
                            }

                            var child_topic_config = this.config.GetTopicConfig(child);
                            if (null != child_topic_config)
                            {
                                ExportObject(
                                    child,
                                    Path.Combine(path, child_topic_config.folder_name_func(child)),
                                    tocWriter,
                                    false);
                            }
                        }
                    }
                }

                if (!is_top)
                {
                    tocWriter.WriteEndElement(); //topicref
                }
            }
        }

        private bool IsObjectEmpty(object obj)
        {
            var topic_config = this.config.GetTopicConfig(obj);
            if (null != topic_config)
            {
                if (!topic_config.diagrams_func(obj).IsEmpty())
                {
                    return false;
                }


                foreach (var body_config in topic_config.body)
                {
                    if (body_config is TableConfig)
                    {
                        var table_config = (TableConfig) body_config;
                        if (!table_config.get_func(obj).IsEmpty())
                        {
                            return false;
                        }

                    }
                    else if (body_config is ListConfig)
                    {
                        var list_config = (ListConfig)body_config;
                        if (!list_config.get_func(obj).IsEmpty())
                        {
                            return false;
                        }

                    }
                }

                var children_config = this.config.GetChildrenConfig(obj);
                if (null != children_config)
                {
                    foreach (var child_config in children_config)
                    {
                        foreach (var child in child_config(obj))
                        {
                            if (!IsObjectEmpty(child))
                            {
                                return false;
                            }
                        }
                    }

                }
            }

            return true;
        }

        private void ExportObjectBody(object element, ExportTopicConfig topic_config, string path, XmlWriter tocWriter)
        {
            var file_name = Path.Combine(root_path, path + ".dita");
            if (file_name.Length > max_path_length)
            {
                MessageBox.Show($"Very long name {file_name}");
                return;
            }

            using (var writer = XmlWriter.Create(file_name, new XmlWriterSettings
            {
                Indent = true
            }))
            {
                writer.WriteStartDocument();

                writer.WriteDocType("topic", "-//OASIS//DTD DITA Topic//EN", "topic.dtd", null);

                writer.WriteStartElement("topic");
                writer.WriteAttributeString("id", topic_config.id_func(element));

                writer.WriteElementString("title", topic_config.title_func(element));

                if (topic_config.description_func != null)
                {
                    writer.WriteElementString("shortdesc", topic_config.description_func(element));
                }

                writer.WriteStartElement("body");

                writer.WriteStartElement("p");
                writer.WriteRaw(topic_config.notes_func(element));
                writer.WriteEndElement();//p

                foreach (Diagram diagram in topic_config.diagrams_func(element))
                {
                    writer.WriteStartElement("fig");
                    writer.WriteAttributeString("id", "eadiagram" + diagram.DiagramID);

                    writer.WriteElementString("title", diagram.Name);

                    writer.WriteStartElement("image");
                    writer.WriteAttributeString("placement", "break");
                    writer.WriteAttributeString("scalefit", "yes");
                    writer.WriteAttributeString("href", "images/" + ExportConfig.Translit(diagram.Name) + ".png");
                    writer.WriteEndElement();//image

                    writer.WriteEndElement();//fig
                }

                foreach (var body_config in topic_config.body)
                {
                    RenderBody(element, body_config, writer);
                }


                writer.WriteEndElement();//body


                writer.WriteEndElement();//topic
                writer.WriteEndDocument();
            }
        }

        private static void RenderBody(object element, object body_config, XmlWriter writer)
        {
            if (body_config is TableConfig)
            {
                RenderTable(element, (TableConfig)body_config, writer);
            }
            else if (body_config is ListConfig)
            {
                RenderList(element, (ListConfig)body_config, writer);
            }
        }

        private static void RenderTable(object element, TableConfig table_config, XmlWriter writer)
        {
            if (!table_config.get_func(element).IsEmpty())
            {
                writer.WriteStartElement("p");

                writer.WriteStartElement("table");
                writer.WriteElementString("title", table_config.title);

                writer.WriteStartElement("tgroup");
                writer.WriteAttributeString("cols", table_config.columns.Count.ToString());

                for (int i = 0; i < table_config.columns.Count; ++i)
                {
                    writer.WriteStartElement("colspec");
                    writer.WriteAttributeString("colname", "c" + i);
                    writer.WriteAttributeString("colwidth", "1*");
                    writer.WriteEndElement(); //colspec
                }

                writer.WriteStartElement("thead");
                writer.WriteStartElement("row");

                foreach (var column in table_config.columns)
                {
                    writer.WriteElementString("entry", column.title);
                }


                writer.WriteEndElement(); //row
                writer.WriteEndElement(); //thead


                writer.WriteStartElement("tbody");

                foreach (var item in table_config.get_func(element))
                {
                    writer.WriteStartElement("row");
                    foreach (var column in table_config.columns)
                    {
                        if (column.Is_Html)
                        {
                            writer.WriteStartElement("entry");
                            writer.WriteRaw(column.get_func(item));
                            writer.WriteEndElement(); //entry
                        }
                        else
                        {
                            writer.WriteElementString("entry", column.get_func(item));
                        }
                    }

                    writer.WriteEndElement(); //row
                }

                writer.WriteEndElement(); //tbody
                writer.WriteEndElement(); //tgroup
                writer.WriteEndElement(); //table
                writer.WriteEndElement(); //p
            }
        }
        private static void RenderList(object element, ListConfig list_config, XmlWriter writer)
        {
            if (!list_config.get_func(element).IsEmpty())
            {
                writer.WriteStartElement("ol");

                foreach (var item in list_config.get_func(element))
                {
                    writer.WriteStartElement("li");
                    writer.WriteString(list_config.title_func(item));

                    foreach (var body_config in list_config.body.body)
                    {
                        RenderBody(item, body_config, writer);
                    }

                    writer.WriteEndElement(); //li
                }

                writer.WriteEndElement(); //ol
            }
        }
    }

    static class IEnumerableExt
    {
        public static bool IsEmpty(this IEnumerable collection)
        {
            foreach (var item in collection)
            {
                return false;
            }

            return true;
        }
    }
}
