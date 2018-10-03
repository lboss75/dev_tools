using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EA;
using Attribute = EA.Attribute;

namespace ea2dita.lib
{
    class ExportConfig
    {
        private Dictionary<Type, List<Func<object, IEnumerable>>> children_func = new Dictionary<Type, List<Func<object, IEnumerable>>>();
        private Dictionary<Type, ExportTopicConfig> topics = new Dictionary<Type, ExportTopicConfig>();
        private Random rnd = new Random();

        public ExportConfig()
        {
            this.AddChildren<Package>(x => x.Elements);
            this.AddChildren<Package>(x => x.Packages);
            this.AddChildren<Element>(x => x.Elements.Cast<Element>().Where(y => y.Type != "Boundary"));

            this.AddTopic<Package>(
                id: x => "pkg" + x.ParentID,
                name: x => x.Name,
                title: x => x.Name,
                notes: x => x.Notes,
                diagrams: x => x.Diagrams,
                folder_name: x => Translit(FirstNotEmpty(x.Alias, x.Name)),
                config: cfg => {

                }
            );

            this.AddTopic<Element>(
                id: x => "eae" + x.ElementID,
                name: x => x.Name,
                title: x => FirstNotEmpty(x.Name, x.Alias, x.ElementID) + "(" + x.Type + ")",
                notes: x => x.Notes,
                diagrams: x => x.Diagrams,
                folder_name: x => Translit(FirstNotEmpty(x.Alias, x.Name, x.PackageID)),
                config: cfg =>
                {
                    cfg.AddTable<Attribute>(x => x.Attributes, "Аттрибуты", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => x.Type);
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Method>(x => x.Methods, "Методы", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => BuildMethodType(x));
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Connector>(x => FilterConnectors(x.Connectors), "Связи", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Направление", x => x.Direction);
                        columns.Add("Тип", x => $"{x.Type}.{x.Stereotype}");
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Requirement>(x => x.Requirements, "Требования", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => x.Type);
                        columns.Add("Статус", x => x.Status);
                        columns.Add("Приоритет", x => x.Priority);
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Constraint>(x => x.Constraints, "Ограничения", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => x.Type);
                        columns.Add("Статус", x => x.Status);
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Issue>(x => x.Issues, "Особенности", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => x.Type);
                        columns.Add("Статус", x => x.Status);
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddTable<Scenario>(x => x.Scenarios, "Сценарии", columns =>
                    {
                        columns.Add("Имя", x => x.Name);
                        columns.Add("Тип", x => x.Type);
                        columns.Add("Примечания", x => x.Notes, Is_Html: true);
                    });

                    cfg.AddList<Scenario>(x => x.Scenarios, "Сценарии", x => x.Name, body =>
                    {
                        body.AddTable<ScenarioStep>(x => x.Steps.Cast<ScenarioStep>().OrderBy(y => y.Pos), "Шаги", columns =>
                        {
                            columns.Add("Шаг", x => x.Pos.ToString() + ((x.StepType == ScenarioStepType.stActor) ? " Actor" : " System"));
                            columns.Add("Имя", x => x.Name);
                            columns.Add("Uses", x => x.Uses);
                            columns.Add("Results", x => x.Results);
                        });
                    });
                });
        }

        public ExportTopicConfig GetTopicConfig(object obj)
        {
            foreach (var topicConfig in topics)
            {
                try
                {
                    var inter = Marshal.GetComInterfaceForObject(obj, topicConfig.Key);
                    return topicConfig.Value;
                }
                catch (InvalidCastException)
                {

                }
            }

            return null;
        }

        public List<Func<object, IEnumerable>> GetChildrenConfig(object obj)
        {
            foreach (var childConfig in children_func)
            {
                try
                {
                    var inter = Marshal.GetComInterfaceForObject(obj, childConfig.Key);
                    return childConfig.Value;
                }
                catch (InvalidCastException)
                {

                }
            }

            return null;
        }

        private void AddTopic<T>(
            Func<T, string> id,
            Func<T, string> name,
            Func<T, string> title,
            Func<T, string> notes,
            Func<T, string> folder_name,
            Func<T, IEnumerable> diagrams = null,
            Func<T, string> description = null,
            Action<TopicConfig<T>> config = null)
        {
            var cfg = new ExportTopicConfig(
                x => id((T)x),
                x => name((T)x),
                x => title((T)x),
                x => notes((T)x),
                x => folder_name((T)x),
                x => (diagrams == null) ? new object[0] :  diagrams((T)x),
                x => description?.Invoke((T)x)
                );

            var c = new TopicConfig<T>(cfg);
            config(c);
            this.topics.Add(typeof(T), cfg);
        }

        private void AddChildren<T>(Func<T, IEnumerable> get_child_func)
        {
            List<Func<object, IEnumerable>> l;
            if (!this.children_func.TryGetValue(typeof(T), out l))
            {
                l = new List<Func<object, IEnumerable>>();
                this.children_func.Add(typeof(T), l);
            }

            l.Add(x => get_child_func((T)x));
        }


        class ColumnConfig<TElement>
        {
            private List<ColumnConfig> columns;

            public ColumnConfig(List<ColumnConfig> columns)
            {
                this.columns = columns;
            }

            public void Add(string title, Func<TElement, string> get_func, bool Is_Html = false)
            {
                this.columns.Add(new ColumnConfig
                {
                    title =  title,
                    get_func = (x => get_func((TElement)x)),
                    Is_Html =  Is_Html
                });
            }
        }

        class TopicConfig<T>
        {
            private ExportTopicConfig cfg;

            public TopicConfig(ExportTopicConfig cfg)
            {
                this.cfg = cfg;
            }

            public void AddTable<TElement>(Func<T, IEnumerable> func, string title, Action<ColumnConfig<TElement>> columns_config)
            {
                var cfg = new TableConfig
                {
                    title = title,
                    get_func = (x => func((T)x))
                };
                columns_config(new ColumnConfig<TElement>(cfg.columns));

                this.cfg.body.Add(cfg);
            }
            public void AddList<TElement>(Func<T, IEnumerable> func, string title, Func<TElement, string> title_func, Action<BodyConfig<TElement>> columns_config)
            {
                var cfg = new ListConfig
                {
                    title = title,
                    get_func = (x => func((T)x)),
                    title_func = (x => title_func((TElement)x))
                };
                columns_config(new BodyConfig<TElement>(cfg.body));

                this.cfg.body.Add(cfg);
            }
        }

        class BodyConfig<TElement>
        {
            private BodyConfig body;

            public BodyConfig(BodyConfig body)
            {
                this.body = body;
            }
            public void AddTable<TItem>(Func<TElement, IEnumerable> func, string title, Action<ColumnConfig<TItem>> columns_config)
            {
                var cfg = new TableConfig
                {
                    title = title,
                    get_func = (x => func((TElement)x))
                };
                columns_config(new ColumnConfig<TItem>(cfg.columns));

                this.body.body.Add(cfg);
            }
        }

        public string FirstNotEmpty(params object[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (candidate != null)
                {
                    var result = candidate.ToString();
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }

            return rnd.Next().ToString();
        }


        private static string BuildMethodType(Method method)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrWhiteSpace(method.ReturnType))
            {
                sb.Append("void ");
            }
            else
            {
                sb.Append(method.ReturnType);
                sb.Append(' ');
            }

            sb.Append('(');
            bool is_first = true;
            foreach (Parameter parameter in method.Parameters)
            {
                if (!is_first)
                {
                    sb.Append(',');
                }
                else
                {
                    is_first = false;
                }

                sb.Append(parameter.Name);
                sb.Append(':');
                sb.Append(parameter.Type);
            }
            sb.Append(')');

            return sb.ToString();
        }

        private static Dictionary<char, string> translit_gost = new Dictionary<char, string>
        {
            {'Є', "EH"},
            {'І', "I"},
            {'і', "i"},
            {'№', "#"},
            {'є', "eh"},
            {'А', "A"},
            {'Б', "B"},
            {'В', "V"},
            {'Г', "G"},
            {'Д', "D"},
            {'Е', "E"},
            {'Ё', "JO"},
            {'Ж', "ZH"},
            {'З', "Z"},
            {'И', "I"},
            {'Й', "JJ"},
            {'К', "K"},
            {'Л', "L"},
            {'М', "M"},
            {'Н', "N"},
            {'О', "O"},
            {'П', "P"},
            {'Р', "R"},
            {'С', "S"},
            {'Т', "T"},
            {'У', "U"},
            {'Ф', "F"},
            {'Х', "KH"},
            {'Ц', "C"},
            {'Ч', "CH"},
            {'Ш', "SH"},
            {'Щ', "SHH"},
            {'Ъ', "'"},
            {'Ы', "Y"},
            {'Ь', ""},
            {'Э', "EH"},
            {'Ю', "YU"},
            {'Я', "YA"},
            {'а', "a"},
            {'б', "b"},
            {'в', "v"},
            {'г', "g"},
            {'д', "d"},
            {'е', "e"},
            {'ё', "jo"},
            {'ж', "zh"},
            {'з', "z"},
            {'и', "i"},
            {'й', "jj"},
            {'к', "k"},
            {'л', "l"},
            {'м', "m"},
            {'н', "n"},
            {'о', "o"},
            {'п', "p"},
            {'р', "r"},
            {'с', "s"},
            {'т', "t"},
            {'у', "u"},
            {'ф', "f"},
            {'х', "kh"},
            {'ц', "c"},
            {'ч', "ch"},
            {'ш', "sh"},
            {'щ', "shh"},
            {'ъ', ""},
            {'ы', "y"},
            {'ь', ""},
            {'э', "eh"},
            {'ю', "yu"},
            {'я', "ya"},
            {'«', ""},
            {'»', ""},
            {'—', "-"},
            {' ', "-"}
        };

        public static string Translit(string value)
        {
            var sb = new StringBuilder();
            foreach (var ch in value)
            {
                string v;
                if (translit_gost.TryGetValue(ch, out v))
                {
                    sb.Append(v);
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private IEnumerable<Connector> FilterConnectors(Collection connectors)
        {
            foreach (Connector connector in connectors)
            {
                if (!string.IsNullOrWhiteSpace(connector.Name) || !string.IsNullOrWhiteSpace(connector.Notes))
                {
                    yield return connector;
                }
            }
        }
    }

    class ColumnConfig
    {
        public string title;
        public Func<object, string> get_func;
        public bool Is_Html;
    }

    class TableConfig
    {
        public string title;
        public Func<object, IEnumerable> get_func;
        public List<ColumnConfig> columns = new List<ColumnConfig>();
    }

    class ListConfig
    {
        public string title;
        public Func<object, IEnumerable> get_func;
        public Func<object, string> title_func;
        public BodyConfig body = new BodyConfig();
    }

    class BodyConfig
    {
        public List<object> body = new List<object>();
    }

    internal class ExportTopicConfig
    {
        public Func<object, string> id_func;
        public Func<object, string> name_func;
        public Func<object, string> title_func;
        public Func<object, string> notes_func;
        public Func<object, string> folder_name_func;
        public Func<object, IEnumerable> diagrams_func;
        public Func<object, string> description_func;

        public List<object> body = new List<object>();

        public ExportTopicConfig(
            Func<object, string> id_func,
            Func<object, string> name_func,
            Func<object, string> title_func,
            Func<object, string> notes_func,
            Func<object, string> folder_name_func,
            Func<object, IEnumerable> diagrams_func,
            Func<object, string> description_func)
        {
            this.id_func = id_func;
            this.name_func = name_func;
            this.title_func = title_func;
            this.notes_func = notes_func;
            this.folder_name_func = folder_name_func;
            this.diagrams_func = diagrams_func;
            this.description_func = description_func;
        }
    }
}
