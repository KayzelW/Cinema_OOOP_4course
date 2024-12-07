using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cinema_OOOP_4course.Lib;
using Microsoft.Win32;
using OfficeOpenXml;

namespace Cinema_OOOP_4course.Excel;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<Cinema> Cinemas { get; set; } = [];
    public ObservableCollection<RoomModel> CurRooms { get; set; } = [];

    public MainWindow()
    {
        InitializeComponent();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        CinemaGrid.ItemsSource = Cinemas;
        RoomGrid.ItemsSource = CurRooms;

        CinemaGrid.BeginningEdit += (sender, args) => { args.Cancel = true; };
        // RoomGrid.BeginningEdit += (sender, args) => { args.Cancel = true; };
    }

    private void AddCinema_Click(object sender, RoutedEventArgs e) =>
        Cinemas.Add(new Cinema());

    // Обновление отображения комнат для выбранного кинотеатра
    private async void CinemaGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CinemaGrid.SelectedItem is Cinema selectedCinema)
        {
            CurRooms.Clear();

            var rows = new ObservableCollection<RoomModel>(selectedCinema.Rooms
                .SelectMany(x => x.Places
                    .Select(y => new RoomModel(x, y.Key))
                )
            );

            foreach (var newRoom in rows)
            {
                CurRooms.Add(newRoom);
            }
        }
    }

    private void AddRoom_Click(object sender, RoutedEventArgs e)
    {
        // Добавление комнаты к выбранному кинотеатру
        if (CinemaGrid.SelectedItem is Cinema selectedCinema)
        {
            selectedCinema.Rooms.Add(new Room()
            {
                Places = new Dictionary<string, int>()
                {
                    { "standard", 0 },
                }
            });
            CinemaGrid_SelectionChanged(null, null);
        }
        else
        {
            MessageBox.Show("Please select a cinema first!");
        }
    }

    private void DeleteRoom_Click(object sender, RoutedEventArgs e)
    {
        // Удаление комнаты у выбранного кинотеатра
        if (CinemaGrid.SelectedItem is Cinema selectedCinema &&
            RoomGrid.SelectedItem is KeyValuePair<string, KeyValuePair<string, int>> selectedRoom)
        {
            var roomToDelete = selectedCinema.Rooms.FirstOrDefault(r => r.Id == selectedRoom.Key);
            if (roomToDelete != null)
            {
                selectedCinema.Rooms.Remove(roomToDelete);
                // CinemaGrid.Items.Refresh();
                CinemaGrid_SelectionChanged(null, null);
            }
        }
        else
        {
            MessageBox.Show("Please select a cinema and a room to delete!");
        }
    }

    #region Excel

    /// <summary>
    ///  Экспорт коллекций в эксель
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExportToExcel_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "Save Excel File",
            FileName = "cinemas.xlsx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var filePath = saveFileDialog.FileName;

            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Cinemas");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Cinema ID";
                worksheet.Cells[1, 2].Value = "Room ID";
                worksheet.Cells[1, 3].Value = "Category";
                worksheet.Cells[1, 4].Value = "Places Count";

                int row = 2;

                foreach (var cinema in Cinemas)
                {
                    foreach (var room in cinema.Rooms)
                    {
                        foreach (var place in room.Places)
                        {
                            worksheet.Cells[row, 1].Value = cinema.Id;
                            worksheet.Cells[row, 2].Value = room.Id;
                            worksheet.Cells[row, 3].Value = place.Key;
                            worksheet.Cells[row, 4].Value = place.Value;
                            row++;
                        }
                    }
                }

                package.SaveAs(new FileInfo(filePath));
                MessageBox.Show("Data successfully exported!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Загрузка данных из экселя в коллекции. Инициализация интерфейса 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LoadFromExcel_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "Select an Excel File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var filePath = openFileDialog.FileName;

            try
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    MessageBox.Show("No worksheet found in the file.");
                    return;
                }

                var newCinemas = new ObservableCollection<Cinema>();
                int row = 2;

                // Чтение данных из файла
                while (worksheet.Cells[row, 1].Value != null)
                {
                    var cinemaId = worksheet.Cells[row, 1].Text;
                    var roomId = worksheet.Cells[row, 2].Text;
                    var category = worksheet.Cells[row, 3].Text;
                    var placesCount = int.TryParse(worksheet.Cells[row, 4].Text, out var count) ? count : 0;

                    // Поиск или создание Cinema
                    var cinema = newCinemas.FirstOrDefault(c => c.Id == cinemaId);
                    if (cinema == null)
                    {
                        cinema = new Cinema { Id = cinemaId };
                        newCinemas.Add(cinema);
                    }

                    // Поиск или создание Room
                    var room = cinema.Rooms.FirstOrDefault(r => r.Id == roomId);
                    if (room == null)
                    {
                        room = new Room { Id = roomId };
                        cinema.Rooms.Add(room);
                    }

                    // Добавление категории мест
                    room.Places[category] = placesCount;

                    row++;
                }

                // Отображение интерфейса
                this.CinemaGrid.Visibility = Visibility.Visible;
                this.AddCinemaBtn.Visibility = Visibility.Visible;
                // this.EditCinemaBtn.Visibility = Visibility.Visible;
                this.ExportToExcelBtn.Visibility = Visibility.Visible;
                this.AddRoomBtn.Visibility = Visibility.Visible;
                this.DeleteRoomBtn.Visibility = Visibility.Visible;
                this.RoomGrid.Visibility = Visibility.Visible;

                this.LoadExcelBtn.Visibility = Visibility.Collapsed;

                // Заменить текущие данные на загруженные
                Cinemas = newCinemas;
                CinemaGrid.ItemsSource = Cinemas;
                CinemaGrid.Items.Refresh();

                MessageBox.Show("Data successfully loaded!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }
    }

    #endregion
}