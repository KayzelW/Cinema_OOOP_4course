using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cinema_OOOP_4course.Lib;

public class Cinema
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public ObservableCollection<Room> Rooms { get; set; } = [];


    public static Room operator +(Cinema cinema, Room room)
    {
        cinema.Rooms.Add(room);
        return room;
    }

    public static bool operator ==(Cinema? cinema1, Cinema? cinema2)
    {
        if (cinema1 is null && cinema2 is null)
        {
            return true;
        }

        if (cinema1 is not null && cinema2 is null)
        {
            return false;
        }
        
        return cinema1.Equals(cinema2)
               || (cinema1.Rooms.Count == cinema2.Rooms.Count
                   && cinema1.Rooms.SequenceEqual(cinema2.Rooms)
               );
    }

    public static bool operator !=(Cinema? cinema1, Cinema? cinema2)
    {
        if (cinema1 is null && cinema2 is null)
        {
            return false;
        }

        if (cinema1 is not null && cinema2 is null)
        {
            return true;
        }
        
        return cinema1.Equals(cinema2)
               && cinema1.Rooms.Count == cinema2.Rooms.Count
               && cinema1.Rooms.SequenceEqual(cinema2.Rooms);
    }
}