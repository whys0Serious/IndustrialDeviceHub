using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Controls.Controls;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class RecipeEditViewModel : ObservableObject, IDialogAware
    {
        private readonly IRecipeService _recipeService;

        // 用于区分是新增还是编辑（如果传入 null 就是新增）
        private int? _editingId;

        public event Action<bool>? RequestClose;

        [ObservableProperty] private string _title = "新增配方";
        [ObservableProperty] private string _name = string.Empty;

        // 用 string 接收用户输入，方便捕获异常格式（防止输入字母或负数）
        [ObservableProperty] private string _targetTemperature = "80.0";
        [ObservableProperty] private string _targetPressure = "1000";
        [ObservableProperty] private string _targetSpeed = "1500";

        [ObservableProperty] private string? _errorMessage;

        public RecipeEditViewModel(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        /// <param name="recipe">如果要编辑，传入已有的配方对象；如果为 null，则为新增</param>
        public void Initialize(Recipe? recipe = null)
        {
            if (recipe != null)
            {
                // 编辑模式：回显数据
                _editingId = recipe.Id;
                Title = "编辑配方";
                Name = recipe.Name;
                TargetTemperature = recipe.TargetTemperature.ToString("F1");
                TargetPressure = recipe.TargetPressure.ToString("F0");
                TargetSpeed = recipe.TargetSpeed.ToString("F0");
            }
            else
            {
                // 新增模式
                _editingId = null;
                Title = "新增配方";
                Name = string.Empty;
                TargetTemperature = "80.0";
                TargetPressure = "1000";
                TargetSpeed = "1500";
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            //基础非空校验
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "配方名称不能为空";
                return;
            }

            // 数值解析与业务校验
            if (!double.TryParse(TargetTemperature, out double temp) || temp <= 0 || temp > 500)
            {
                ErrorMessage = "目标温度必须是 0 ~ 500 之间的有效数字";
                return;
            }

            if (!double.TryParse(TargetPressure, out double press) || press <= 0 || press > 5000)
            {
                ErrorMessage = "目标压力必须是 0 ~ 5000 之间的有效数字";
                return;
            }

            if (!double.TryParse(TargetSpeed, out double speed) || speed <= 0 || speed > 10000)
            {
                ErrorMessage = "目标转速必须是 0 ~ 10000 之间的有效数字";
                return;
            }

            try
            {
                
                var recipe = new Recipe
                {
                    Name = Name.Trim(),
                    TargetTemperature = temp,
                    TargetPressure = press,
                    TargetSpeed = speed
                };

                // 新增/编辑保存
                if (_editingId.HasValue)
                {
                    recipe.Id = _editingId.Value;

                    await _recipeService.UpdateAsync(recipe); 
                }
                else
                {
                    await _recipeService.AddAsync(recipe);
                }

                // 保存成功，关闭弹窗并返回 true
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"保存失败：{ex.Message}";
            }
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
