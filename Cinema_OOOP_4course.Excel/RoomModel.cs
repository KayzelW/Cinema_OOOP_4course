namespace Cinema_OOOP_4course.Lib;

public class RoomModel
{
    public string Id { get; set; }

    public string Key
    {
        get => _key;
        set
        {
            var placeValue = Value; //текущее кол-во мест
            Room.Places.Remove(_key); //удаление старого значения ключа
            _key = value; //сохранение в модели нового
            Room.Places[Key] = placeValue; // восстановление целостности
        }
    }

    private string _key;

    public int Value
    {
        get => Room.Places[Key];
        set => Room.Places[Key] = value;
    }

    private Room Room { get; }

    public RoomModel(Room room, string key)
    {
        this.Id = room.Id;
        this._key = key;
        Room = room;
    }
}