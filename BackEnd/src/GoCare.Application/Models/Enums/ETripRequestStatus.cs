namespace GoCare.Application.Models.Enums;

public enum ETripRequestStatus // ciclo di vita di una richiesta di trasporto
{
    Pending, 
    Confirmed,
    InProgress,
    Completed,
    NotCovered,
    Cancelled
}
