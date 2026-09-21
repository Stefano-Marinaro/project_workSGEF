namespace GoCare.Models.Enums;

public enum ETripTransitionStatus // ciclo di vita di un trasporto
{
    Pending,
    InCharge,
    Arriving,
    OnSite,
    Returning,
    Suspended, // guasto/imprevisto durante il viaggio (PA-07): raggiungibile da InCharge/Arriving/OnSite/Returning, non da Pending né da Completed
    Completed
}
