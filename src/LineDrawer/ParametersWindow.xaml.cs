using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace LineDrawer
{
    public partial class ParametersWindow : Window
    {
        private readonly MainWindow mainWindow;

        public ParametersWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.DataContext = mainWindow.Model;
            
            // Инициализируем текущий пресет, если есть
            if (mainWindow.Model.CurrentPreset != null)
                PresetsComboBox.SelectedItem = mainWindow.Model.CurrentPreset;
            else if (mainWindow.Model.Presets.Count > 0)
                PresetsComboBox.SelectedItem = mainWindow.Model.Presets.First();
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            mainWindow.Reset();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void PresetsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainWindow.OnPresetSelectionChanged(sender, e);
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                Title = "Сохранить все пресеты"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(mainWindow.Model.Presets.ToArray(), Formatting.Indented);
                    File.WriteAllText(saveDialog.FileName, json);
                    
                    MessageBox.Show("Все пресеты успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = "json",
                Title = "Загрузить все пресеты"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(openDialog.FileName);
                    var presets = JsonConvert.DeserializeObject<ProducerModelInfo[]>(json);
                    
                    if (presets != null && presets.Length > 0)
                    {
                        // Очищаем текущие пресеты
                        mainWindow.Model.Presets.Clear();
                        
                        // Добавляем загруженные пресеты
                        foreach (var preset in presets)
                        {
                            mainWindow.Model.Presets.Add(preset);
                        }
                        
                        // Устанавливаем первый пресет как текущий
                        mainWindow.Model.CurrentPreset = presets[0];
                        PresetsComboBox.SelectedItem = mainWindow.Model.CurrentPreset;
                        
                        // Очищаем текущие суставы и загружаем из первого пресета
                        mainWindow.Model.Joints.Clear();
                        foreach (var joint in presets[0].Joints)
                        {
                            mainWindow.Model.Joints.Add(joint);
                        }
                        
                        // Перезапускаем отрисовку
                        mainWindow.Reset();
                        
                        MessageBox.Show($"Загружено {presets.Length} пресетов!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Файл не содержит пресетов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
} 