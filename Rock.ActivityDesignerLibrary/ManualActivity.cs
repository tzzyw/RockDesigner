﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation;
using System.Windows;
using Microsoft.CSharp.Activities;
using Rock.Orm.Common;
using Microsoft.VisualBasic.Activities;
using System.Transactions;

namespace Rock.ActivityDesignerLibrary
{
    [Designer(typeof(ManualActivityDesigner))]
    public sealed class ManualActivity : NativeActivity<string>, IActivityTemplateFactory
    {
        [Description("工作流实例ID")]
        [Browsable(true)]
        public InArgument<int> WorkflowInstanceID { get; set; }

        [Description("类型，包含角色和用户")]
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<int> Type { get; set; }

        [Description("选择项")]
        [RequiredArgument]
        [Browsable(true)]
        public InArgument<int> Item { get; set; }

        [Description("职位")]
        [Browsable(true)]
        public InArgument<int> Position { get; set; }

        [Description("发起人")]
        [RequiredArgument]
        [Browsable(false)]
        public InArgument<string> FirstActor { get; set; }

        [Description("上一步执行者")]
        [RequiredArgument]
        [Browsable(false)]
        public InOutArgument<string> LastActor { get; set; }

        [Description("窗口命令")]
        [RequiredArgument]
        public InArgument<string> Command { get; set; }

        [Description("参数")]
        [RequiredArgument]
        [Browsable(true)]
        public InOutArgument<Dictionary<string, object>> ExchangeParams { get; set; }

        [Description("下一步条件，多个条件使用^分割")]
        [RequiredArgument]
        public InArgument<string> Expression { get; set; }

        [Description("描述")]
        [RequiredArgument]
        public InArgument<string> Description { get; set; }

        [Description("工作流活动ID")]
        [Browsable(true)]
        public int WorkflowActivityID { get; set; }

        /// <summary>
        /// 对于 NativeActivity 派生的活动，若其调用 
        /// System.Activities.NativeActivityContext 中定义的某个 CreateBookmark 重载，
        /// 以便执行异步操作，则必须覆盖 CanInduceIdle 属性且返回 true
        /// </summary>
        protected override bool CanInduceIdle
        {
            get
            { return true; }
        }
        public Activity Create(DependencyObject target)
        {
            ManualActivityGuide customActivityGuide = new ManualActivityGuide();
            bool? result = customActivityGuide.ShowDialog();

            if (result == true)
            {
                ManualActivity activity = new ManualActivity();

                activity.DisplayName = "人工活动";

                activity.Type = Convert.ToInt32(customActivityGuide.cbxTypes.SelectedValue);
                activity.Item = Convert.ToInt32(customActivityGuide.cbxItems.SelectedValue);
                activity.Position = customActivityGuide.cbxPosition.SelectedValue == null ? -99 : Convert.ToInt32(customActivityGuide.cbxPosition.SelectedValue);
                activity.Command = customActivityGuide.txtCmd.Text;
                activity.Expression = customActivityGuide.Expression;
                activity.Description = customActivityGuide.txtDescription.Text;
                activity.WorkflowActivityID = customActivityGuide.WorkflowActivityID;

                VisualBasicValue<int> workflowfInstanceIDParams = new VisualBasicValue<int>();
                workflowfInstanceIDParams.ExpressionText = "WorkflowInstanceID";
                activity.WorkflowInstanceID = workflowfInstanceIDParams;

                activity.ExchangeParams = new VisualBasicReference<Dictionary<string, object>> { ExpressionText = "ExchangeParams" };
                activity.FirstActor = new VisualBasicValue<string> { ExpressionText = "FirstActor" };
                activity.LastActor = new VisualBasicReference<string> { ExpressionText = "LastActor" };

                return activity;
            }
            return null;

        }
        protected override void Execute(NativeActivityContext context)
        {
            DesignService designService = new DesignService();
            //启动事务
            using (TransactionScope trans = new TransactionScope())
            {
                //添加工作流活动实例
                DynEntity workflowfActivityInstance = new DynEntity("WorkflowActivityInstance");
                workflowfActivityInstance["WorkflowActivityInstanceID"] = designService.GetNextID("WorkflowActivityInstance");
                workflowfActivityInstance["WorkflowInstanceID"] = context.GetValue(WorkflowInstanceID);
                workflowfActivityInstance["WorkflowActivityID"] = WorkflowActivityID;
                workflowfActivityInstance["WorkflowActivityInstanceName"] = DisplayName;
                workflowfActivityInstance["State"] = "正在执行";
                workflowfActivityInstance["StartTime"] = DateTime.Now;
                designService.AddDynEntity(workflowfActivityInstance);

                //获取工作流实例
                DynEntity workflowInstance = designService.GetDynEntityByID("WorkflowInstance", context.GetValue(WorkflowInstanceID));
                int workflowID;
                if (workflowInstance != null)
                {
                    workflowID = Convert.ToInt32(workflowInstance["WorkflowID"]);
                }
                else
                {
                    throw new ApplicationException(string.Format("{0}实例下的活动{1}，根据工作流实例ID获取工作流实例不正常", context.GetValue(WorkflowInstanceID), DisplayName));
                }

                //添加待做任务清单
                DynEntity task = new DynEntity("WorkflowToDoList");
                task["WorkflowToDoListID"] = designService.GetNextID("WorkflowToDoList");
                task["Type"] = context.GetValue(Type).ToString();
                task["Item"] = context.GetValue(Item).ToString();
                task["Position"] = context.GetValue(Position).ToString();
                task["FirstActor"] = context.GetValue(FirstActor);
                task["LastActor"] = context.GetValue(LastActor);

                string command = context.GetValue(Command);
                Dictionary<string, object> inParams = this.ExchangeParams.Get(context) as Dictionary<string, object>;
                foreach (string key in inParams.Keys)
                {
                    if (inParams[key] != null)
                    {
                        command = command.Replace("@" + key + "@", inParams[key].ToString());
                    }
                }

                task["WorkflowToDoListName"] = DisplayName;
                task["WorkflowID"] = workflowID;
                task["WorkflowInstanceID"] = context.GetValue(WorkflowInstanceID);
                task["WorkflowActivityInstanceID"] = workflowfActivityInstance["WorkflowActivityInstanceID"];
                task["Expression"] = context.GetValue(Expression);
                task["BookmarkName"] = workflowfActivityInstance["WorkflowActivityInstanceID"].ToString();
                task["Command"] = command;
                task["Comment"] = context.GetValue(Description);
                task["State"] = "待处理";

                if (inParams.ContainsKey("TableName"))
                {
                    task["TableName"] = inParams["TableName"].ToString();
                }
                if (inParams.ContainsKey("TableKey"))
                {
                    task["TableKey"] = inParams["TableKey"].ToString();
                }
                designService.AddDynEntity(task);
                context.CreateBookmark(workflowfActivityInstance["WorkflowActivityInstanceID"].ToString(), new BookmarkCallback(BookmarkCallbackMethod));

                trans.Complete();
            }
        }

        void BookmarkCallbackMethod(NativeActivityContext context, Bookmark bookmark, object obj)
        {
            if (obj != null && obj is Dictionary<string, object>)
            {
                Dictionary<string, object> outParams = obj as Dictionary<string, object>;

                if (!outParams.ContainsKey("LastActor"))
                {
                    this.LastActor.Set(context, outParams["LastActor"].ToString());
                    outParams.Remove("LastActor");
                }

                Dictionary<string, object> inParams = this.ExchangeParams.Get(context) as Dictionary<string, object>;

                foreach (string key in outParams.Keys)
                {
                    inParams[key] = outParams[key];
                }

                this.ExchangeParams.Set(context, inParams);
            }
        }
    }
}