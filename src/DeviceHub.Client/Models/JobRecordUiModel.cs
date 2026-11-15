using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Models
{
    /// <summary>
    /// 包装 Core 层的 JobRecord
    /// </summary>
    public partial class JobRecordUiModel : ObservableObject
    {

        public JobRecord Record { get; }

        public JobRecordUiModel(JobRecord record)
        {
            Record = record;
        }

        // 暴露给界面的只读属性
        public string JobNo => Record.JobNo;
        public string RecipeName => Record.RecipeName;
        public DateTime StartTime => Record.StartTime;
        public DateTime? EndTime => Record.EndTime;
        public double AvgTemp => Record.AvgTemp;
        public bool IsQualified => Record.IsQualified;


        // 界面的勾选状态
        [ObservableProperty]
        private bool _isSelected;
    }
}
