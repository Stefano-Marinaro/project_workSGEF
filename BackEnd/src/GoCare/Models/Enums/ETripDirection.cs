namespace GoCare.Models.Enums;

public enum ETripDirection
{
    OneWay, // sola andata o solo ritorno: la direzione fisica è già data da StartAddress/EndAddress
    RoundTrip, //andata e ritorno
}
