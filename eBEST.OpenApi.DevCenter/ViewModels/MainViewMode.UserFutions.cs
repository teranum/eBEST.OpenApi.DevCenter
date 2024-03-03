using App.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using eBEST.OpenApi.DevCenter.Components;
using eBEST.OpenApi.DevCenter.Controls;
using eBEST.OpenApi.DevCenter.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO.Compression;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    [ObservableProperty]
    bool _IsUserContentVisibled;

    object? _userContent;
    public object? UserContent
    {
        get => _userContent;
        set
        {
            if (_userContent != value)
            {
                if (_userContent is IUserTool userTool)
                {
                    userTool.CloseTool();
                }
                _userContent = value;
                OnPropertyChanged();
                IsUserContentVisibled = _userContent != null;
            }
        }
    }

    async void LoadUserCustomItems()
    {
        var OrgItems = TabTreeDatas![2].OrgItems!;
        const int image_Group = 1;
        const int image_Item = 2;

        var samplePackageRepo = new GithupRepoHelper("teranum", "ebest-openapi-samples");
        var CSharp샘플 = new IdTextItem(image_Group, "C# 샘플");
        var 파이썬샘플 = new IdTextItem(image_Group, "파이썬 샘플");

        try
        {
            var zipdata = await samplePackageRepo.Download();
            if (zipdata is not null)
            {
                using ZipArchive zipArchive = new(zipdata);
                const string csharp_path = "/CSharp/";
                const string python_path = "/Python/";
                var csharp_smaples = zipArchive.Entries.Where(x => x.FullName.Contains(csharp_path) && (x.FullName.EndsWith(".cs")));
                var python_smaples = zipArchive.Entries.Where(x => x.FullName.Contains(python_path) && (x.FullName.EndsWith(".py") || x.FullName.EndsWith(".ui")));
                foreach (var sample in csharp_smaples)
                {
                    using var stream = sample.Open();
                    byte[] buffer = new byte[sample.Length];
                    int nReadCount = stream.Read(buffer, 0, buffer.Length);
                    string content = Encoding.UTF8.GetString(buffer, 0, nReadCount);
                    int nTitlePos = sample.FullName.IndexOf(csharp_path) + csharp_path.Length;
                    var item = new IdTextItem(image_Item, sample.FullName[nTitlePos..])
                    {
                        Tag = content,
                    };
                    CSharp샘플.AddChild(item);
                }
                foreach (var sample in python_smaples)
                {
                    using var stream = sample.Open();
                    byte[] buffer = new byte[sample.Length];
                    int nReadCount = stream.Read(buffer, 0, buffer.Length);
                    string content = Encoding.UTF8.GetString(buffer, 0, nReadCount);
                    int nTitlePos = sample.FullName.IndexOf(python_path) + python_path.Length;
                    var item = new IdTextItem(image_Item, sample.FullName[nTitlePos..])
                    {
                        Tag = content,
                    };
                    파이썬샘플.AddChild(item);
                }
            }
        }
        catch
        {
        }

        CSharp샘플.IsExpanded = true;
        파이썬샘플.IsExpanded = true;
        OrgItems.Add(CSharp샘플);
        OrgItems.Add(파이썬샘플);
    }

    BindableAvalonEditor? _csharp_source_view;
    BindableAvalonEditor? _python_source_view;
    void SelectUserCustom(IdTextItem selectItem)
    {
        if (selectItem.Parent is not null)
        {
            if (selectItem.Tag is string content)
            {
                if (selectItem.Parent.Name.Equals("C# 샘플"))
                {
                    if (_csharp_source_view == null)
                    {
                        ContextMenu contextMenu = new();
                        contextMenu.Items.Add(new MenuItem { Header = "복사", Command = ApplicationCommands.Copy });
                        contextMenu.Items.Add(new Separator());
                        contextMenu.Items.Add(new MenuItem { Header = "모두 선택", Command = ApplicationCommands.SelectAll });
                        _csharp_source_view = new BindableAvalonEditor
                        {
                            FontFamily = new("Consolas"),
                            IsReadOnly = true,
                            BorderBrush = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new System.Windows.Thickness(1),
                            ContextMenu = contextMenu,
                        };
                        var typeConverter = new HighlightingDefinitionTypeConverter();
                        if (typeConverter.ConvertFrom("C#") is IHighlightingDefinition csSyntaxHighlighter)
                            _csharp_source_view.SyntaxHighlighting = csSyntaxHighlighter;
                    }
                    _csharp_source_view.Text = content;
                    MainPartTitle = selectItem.Name;
                    UserContent = _csharp_source_view;
                }
                else if (selectItem.Parent.Name.Equals("파이썬 샘플"))
                {
                    if (_python_source_view == null)
                    {
                        ContextMenu contextMenu = new();
                        contextMenu.Items.Add(new MenuItem { Header = "복사", Command = ApplicationCommands.Copy });
                        contextMenu.Items.Add(new Separator());
                        contextMenu.Items.Add(new MenuItem { Header = "모두 선택", Command = ApplicationCommands.SelectAll });
                        _python_source_view = new BindableAvalonEditor
                        {
                            FontFamily = new("Consolas"),
                            IsReadOnly = true,
                            BorderBrush = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new System.Windows.Thickness(1),
                            ContextMenu = contextMenu,
                        };
                        var typeConverter = new HighlightingDefinitionTypeConverter();
                        if (typeConverter.ConvertFrom("Python") is IHighlightingDefinition csSyntaxHighlighter)
                            _python_source_view.SyntaxHighlighting = csSyntaxHighlighter;
                    }
                    _python_source_view.Text = content;
                    MainPartTitle = selectItem.Name;
                    UserContent = _python_source_view;
                }
            }
        }
    }
}

