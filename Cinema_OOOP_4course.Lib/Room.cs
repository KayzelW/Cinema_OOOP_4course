namespace Cinema_OOOP_4course.Lib;

public class Room
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // категория - места
    public Dictionary<string, int> Places { get; set; } = [];

    public static bool operator ==(Room? room1, Room? room2)
    {
        if (room1 is null && room2 is null)
        {
            return true;
        }

        if (room1 is not null && room2 is null)
        {
            return false;
        }

        return room1!.Equals(room2) // ссылки (вдруг один и тот же объект)
               || room1.Places.All(x =>
                   room2!.Places.ContainsKey(x.Key) &&
                   room2.Places[x.Key] == x.Value) // все ключи и значения по ним равны
            ;
    }

    public static bool operator !=(Room? room1, Room? room2)
    {
        if (room1 is null && room2 is null)
        {
            return false;
        }

        if (room1 is not null && room2 is null)
        {
            return true;
        }

        return !room1!.Places.All(x =>
                   room2!.Places.ContainsKey(x.Key) &&
                   room2.Places[x.Key] == x.Value) // все ключи и значения по ним равны
               || !room1.Equals(room2) // ссылки (вдруг один и тот же объект)
            ;
    }

    public override bool Equals(object? obj)
    {
        return obj is Room room
               && (
                   base.Equals(room) // reference equals
                   || room == this // call operator ==
               );
    }
}