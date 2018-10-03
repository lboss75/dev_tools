using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.Office.Interop.MSProject;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Exception = System.Exception;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;

namespace ProjectTFS
{
    public partial class RibbonMenu
    {
        private void RibbonMenu_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void refreshBtn_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var saveCalculation = app.Calculation;
                var saveScreenUpdating = app.ScreenUpdating;
                var saceEnableChangeHighlighting = app.EnableChangeHighlighting;
                var saveUndoLevels = app.UndoLevels;

                try
                {
                    app.Calculation = PjCalculation.pjManual;
                    app.ScreenUpdating = false;
                    app.EnableChangeHighlighting = false;
                    app.UndoLevels = 1;

                    var tasks = new Dictionary<int, Task>();
                    foreach (Task task in app.ActiveProject.Tasks)
                    {
                        if (task != null)
                        {
                            if (!string.IsNullOrWhiteSpace(task.Text1))
                            {
                                int taskId;
                                if (int.TryParse(task.Text1, out taskId))
                                {
                                    tasks.Add(taskId, task);
                                    continue;
                                }
                            }

                            Marshal.ReleaseComObject(task);
                        }
                    }

                    var tfsUri = new Uri("https://tfs.university.innopolis.ru/tfs/");
                    var tfs = new TfsTeamProjectCollection(tfsUri);
                    var store = (WorkItemStore) tfs.GetService(typeof(WorkItemStore));
                    var project = store.Projects["aeronet"];

                    var iterations = LoadIterations(tfs, project);

                    //Build hierarchy
                    var tasksSnapshot = tasks.ToList();
                    int taskIndex = 0;
                    foreach (var task in tasksSnapshot)
                    {
                        if (0 == taskIndex++ % 100)
                        {
                            app.StatusBar = $"Updating {taskIndex} of {tasksSnapshot.Count}";
                        }

                        WorkItem workItem;
                        try
                        {
                            workItem = store.GetWorkItem(task.Key);
                        }
                        catch (Exception ex)
                        {
                            app.Message(
                                ex.Message + " at update item " + task.Key,
                                PjMessageType.pjOKOnly);
                            return;
                        }

                        try
                        {
                            UpdateTask(task.Value, workItem, iterations);
                            foreach (Link itemLink in workItem.Links)
                            {
                                if (itemLink.ArtifactLinkType.Name == "Related Workitem"
                                    && ((RelatedLink) itemLink).LinkTypeEnd.ImmutableName ==
                                    "System.LinkTypes.Hierarchy-Forward")
                                {
                                    if (!tasks.ContainsKey(((RelatedLink) itemLink).RelatedWorkItemId))
                                    {
                                        var index =
                                            Globals.ThisAddIn.Application.ActiveProject.GetTaskIndexByGuid(task.Value
                                                .Guid);
                                        AddChild(store, iterations, tasks, task.Value,
                                            ((RelatedLink) itemLink).RelatedWorkItemId,
                                            index + 1);
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            throw new Exception($"{ex.Message} для задачи {task.Key}");
                        }
                    }
                }
                finally
                {
                    app.Calculation = saveCalculation;
                    app.ScreenUpdating = saveScreenUpdating;
                    app.EnableChangeHighlighting = saceEnableChangeHighlighting;
                    app.UndoLevels = saveUndoLevels;
                }
            }
            catch (Exception ex)
            {
                Globals.ThisAddIn.Application.Message(
                    ex.Message,
                    PjMessageType.pjOKOnly);
            }
        }

        class IterationInfo
        {
            public DateTime? StartDate;
            public DateTime? FinishDate;

            public IterationInfo(NodeInfo nodeInfo)
            {
                this.StartDate = nodeInfo.StartDate;
                this.FinishDate = nodeInfo.FinishDate;
            }
        }

        private Dictionary<string, IterationInfo> LoadIterations(TfsTeamProjectCollection tfs, Project project)
        {
            var css = tfs.GetService<ICommonStructureService4>();

            var structures = css.ListStructures(project.Uri.ToString());
            var iterations = structures.First(n => n.StructureType.Equals("ProjectLifecycle"));
            var iterationsTree = css.GetNodesXml(new[] { iterations.Uri }, true);

            var result = new Dictionary<string, IterationInfo>();
            BuildIterationTree(result, project.IterationRootNodes, css);

            return result;
        }

        private void BuildIterationTree(Dictionary<string, IterationInfo> result, NodeCollection nodes, ICommonStructureService4 css)
        {
            foreach (Node node in nodes)
            {
                var nodeInfo = css.GetNode(node.Uri.ToString());
                result.Add(node.Path, new IterationInfo(nodeInfo));

                BuildIterationTree(result, node.ChildNodes, css);
            }
        }


        private static int AddChild(
            WorkItemStore store,
            Dictionary<string, IterationInfo> iterations,
            Dictionary<int, Task> tasks,
            Task parentTask,
            int workItemId,
            int index)
        {
            var childWorkItem = store.GetWorkItem(workItemId);
            Task new_task;
            try
            {
                new_task = (index >= Globals.ThisAddIn.Application.ActiveProject.Tasks.Count)
                    ? Globals.ThisAddIn.Application.ActiveProject.Tasks.Add(childWorkItem.Title)
                    : Globals.ThisAddIn.Application.ActiveProject.Tasks.Add(childWorkItem.Title, index);
            }
            catch (Exception ex)
            {
                Globals.ThisAddIn.Application.Message(
                    ex.Message + " at add work item " + workItemId,
                    PjMessageType.pjOKOnly);
                return 0;
            }

            new_task.Text1 = workItemId.ToString();
            while (new_task.OutlineLevel < parentTask.OutlineLevel + 1)
            {
                new_task.OutlineIndent();
            }

            while (new_task.OutlineLevel > parentTask.OutlineLevel + 1)
            {
                new_task.OutlineOutdent();
            }

            UpdateTask(new_task, childWorkItem, iterations);
            tasks.Add(workItemId, new_task);

            int result = 1;
            foreach (Link itemLink in childWorkItem.Links)
            {
                if (itemLink.ArtifactLinkType.Name == "Related Workitem"
                    && ((RelatedLink) itemLink).LinkTypeEnd.ImmutableName ==
                    "System.LinkTypes.Hierarchy-Forward") {
                    if (!tasks.ContainsKey(((RelatedLink) itemLink).RelatedWorkItemId))
                    {
                        result += AddChild(store, iterations, tasks, new_task, ((RelatedLink) itemLink).RelatedWorkItemId, index + result);
                    }
                }
            }

            return result;
        }

        static Task GetNextTask(Task task)
        {
            if (task.OutlineParent == null || task.OutlineParent.UniqueID == task.UniqueID)
            {
                return null;

            }

            bool expectNextTask = false;
            foreach (Task outlineChild in task.OutlineParent.OutlineChildren)
            {
                if (outlineChild.UniqueID == task.UniqueID)
                {
                    expectNextTask = true;
                }
                else if (expectNextTask)
                {
                    return outlineChild;
                }

                Marshal.ReleaseComObject(outlineChild);
            }

            return GetNextTask(task.OutlineParent);
        }

        static void UpdateTask(Task task, WorkItem workItem, Dictionary<string, IterationInfo> iterations)
        {
            if (task.Name != $"{workItem.Id}. {workItem.Title}")
            {
                task.Name = $"{workItem.Id}. {workItem.Title}";
            }

            if (task.Manual)
            {
                task.Manual = false;
            }

            if ((bool)task.Summary)
            {
                if (!string.IsNullOrWhiteSpace(task.ResourceNames))
                {
                    task.ResourceNames = "";
                }

                if (task.Baseline10Work != 0)
                {
                    task.Baseline10Work = 0;
                }
            }
            else
            {
                if (task.ResourceNames != workItem.Fields[CoreField.AssignedTo].Value.ToString())
                {
                    task.ResourceNames = workItem.Fields[CoreField.AssignedTo].Value.ToString();
                }


                IterationInfo iteration;
                if (iterations.TryGetValue(workItem.IterationPath, out iteration))
                {
                    if (iteration.StartDate != null && task.Start != iteration.StartDate.Value)
                    {
                        task.Start = iteration.StartDate.Value;
                    }

                    if (iteration.FinishDate != null && task.Finish != iteration.FinishDate.Value)
                    {
                        task.Finish = iteration.FinishDate.Value;
                    }
                }

                int value;
                TryGetField(workItem, "Microsoft.VSTS.Scheduling.OriginalEstimate", out value);
                if (task.Baseline10Work != value * 60)
                {
                    task.Baseline10Work = value * 60;
                }

                TryGetField(workItem, "Microsoft.VSTS.Scheduling.CompletedWork", out value);
                if (task.ActualWork != value * 60)
                {
                    task.ActualWork = value * 60;
                }

                TryGetField(workItem, "Microsoft.VSTS.Scheduling.RemainingWork", out value);
                if (task.RemainingWork != value * 60)
                {
                    task.RemainingWork = value * 60;
                }
            }


            if (task.Hyperlink !=
                $"https://tfs.university.innopolis.ru/tfs/DefaultCollection/aeronet/_workitems?id={workItem.Id}")
            {

                task.Hyperlink = $"https://tfs.university.innopolis.ru/tfs/DefaultCollection/aeronet/_workitems?id={workItem.Id}";
            }

            if (task.Text2 != workItem.AreaPath)
            {
                task.Text2 = workItem.AreaPath;
            }

            if (task.Text3 != workItem.IterationPath)
            {
                task.Text3 = workItem.IterationPath;
            }

            if (task.Text4 != workItem.Type.Name)
            {
                task.Text4 = workItem.Type.Name;
            }

            if (task.Text5 != workItem.State)
            {
                task.Text5 = workItem.State;
            }
        }

        private static bool TryGetField(WorkItem workItem, string name, out int value)
        {
            if (workItem.Fields.Contains(name))
            {
                var f = workItem.Fields[name];
                if (f?.Value != null)
                {
                    if (int.TryParse(f.Value.ToString(), out value))
                    {
                        return true;
                    }

                    double v;
                    if (double.TryParse(f.Value.ToString().Replace(',', '.'), out v))
                    {
                        value = (int) v;
                        return true;
                    }
                }
            }

            value = 0;
            return false;
        }


        private void compareWithBtn_Click(object sender, RibbonControlEventArgs e)
        {
            var tasks = new Dictionary<int, Task>();
            foreach (Task task in Globals.ThisAddIn.Application.ActiveProject.Tasks)
            {
                if (task != null)
                {
                    if (!string.IsNullOrWhiteSpace(task.Text1))
                    {
                        int taskId;
                        if (int.TryParse(task.Text1, out taskId))
                        {
                            tasks.Add(taskId, task);
                            continue;
                        }
                    }

                    Marshal.ReleaseComObject(task);
                }
            }

            var tfsUri = new Uri("https://tfs.university.innopolis.ru/tfs/");
            var tfs = new TfsTeamProjectCollection(tfsUri);
            var store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
            var project = store.Projects["aeronet"];
            var iterations = LoadIterations(tfs, project);

            var context = new Dictionary<string, string>();
            context.Add("project", "aeronet");

            var toAdd = new Dictionary<int, Task>();

            foreach (var hierarchy in project.QueryHierarchy)
            {
                var queryDef = FindQuery((QueryFolder)hierarchy, "Задачи итерации");
                if (queryDef != null)
                {
                    var query = new Query(store, queryDef.QueryText, context);


                    if (queryDef.QueryType == QueryType.List)
                    {
                        foreach (WorkItem workItem in query.RunQuery())
                        {
                            AddWorkItem(store, workItem, tasks, iterations);
                        }
                    }
                    else
                    {
                        foreach (var linkInfo in query.RunLinkQuery())
                        {
                            var workItem = store.GetWorkItem(linkInfo.TargetId);
                            AddWorkItem(store, workItem, tasks, iterations);
                        }
                    }
                }
            }
        }

        private void AddWorkItem(WorkItemStore store, WorkItem workItem, Dictionary<int, Task> tasks, Dictionary<string, IterationInfo> iterations)
        {
            if (tasks.ContainsKey(workItem.Id))
            {
                return;
            }

            var task = Globals.ThisAddIn.Application.ActiveProject.Tasks.Add(workItem.Title);
            UpdateTask(task, workItem, iterations);
        }

        private static QueryDefinition FindQuery(QueryFolder folders, string queryName)
        {
            var result = (QueryDefinition)folders.FirstOrDefault(o => o.Name.Equals(queryName));
            if (null != result)
            {
                return result;
            }

            foreach (var subFolder in folders)
            {
                if (subFolder is QueryFolder)
                {
                    result = FindQuery((QueryFolder)subFolder, queryName);
                    if (null != result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
