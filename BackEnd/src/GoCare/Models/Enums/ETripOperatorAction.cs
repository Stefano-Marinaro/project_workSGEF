namespace GoCare.Models.Enums;

public enum ETripOperatorAction
{
    Advance, // passa allo stato successivo della sequenza
    Suspend, // segnala un imprevisto durante l'esecuzione
    Resume,  // riprende dallo stesso stato in cui si era fermato
}
