using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Services;
using DeviceHub.Core.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace DeviceHub.Client.ViewModels;

[ViewModel(ServiceLifetime.Transient)]
public partial class AiChatViewModel : ObservableObject
{
    private readonly IAiClientService _aiClient;

    public AiChatViewModel(IAiClientService aiClient)
    {
        _aiClient = aiClient;
    }

    /// <summary>
    /// 用户输入的故障描述
    /// </summary>
    [ObservableProperty]
    private string _faultDescription = string.Empty;

    /// <summary>
    /// AI建议
    /// </summary>
    [ObservableProperty]
    private string? _suggestion;

    /// <summary>
    /// 相似历史案例
    /// </summary>
    public ObservableCollection<SimilarCaseDto> SimilarCases { get; } = new();

    /// <summary>
    /// 是否正在请求
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 错误信息
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// 是否有结果
    /// </summary>
    public bool HasResult => !string.IsNullOrWhiteSpace(Suggestion);

    /// <summary>
    /// 是否有相似案例
    /// </summary>
    public bool HasSimilarCases => SimilarCases.Count > 0;

    [RelayCommand]
    private async Task DiagnoseAsync()
    {
        if (string.IsNullOrWhiteSpace(FaultDescription))
        {
            ErrorMessage = "请输入故障描述";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        Suggestion = null;
        SimilarCases.Clear();
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(HasSimilarCases));

        try
        {
            var result = await _aiClient.DiagnoseAsync(FaultDescription.Trim());

            Suggestion = result.Suggestion;

            SimilarCases.Clear();
            foreach (var c in result.SimilarCases) SimilarCases.Add(c);

            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(HasSimilarCases));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        FaultDescription = string.Empty;
        Suggestion = null;
        SimilarCases.Clear();
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(HasSimilarCases));
    }
}