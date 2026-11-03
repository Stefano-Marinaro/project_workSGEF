namespace GoCare.Models.Domain;

// Come Address, ma con le coordinate obbligatorie: usato per gli indirizzi di un
// TransportRequest (partenza/destinazione/ritorno) e per la sede di un'Association,
// dove servono per calcolare il percorso stradale (prezzo del trasporto, km per
// l'associazione per raggiungere il punto di partenza). Gli altri indirizzi
// dell'app (profilo caregiver, assistito, destinazioni salvate) continuano a
// usare Address, senza coordinate obbligatorie.
public sealed record TripAddress(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province,
    double Latitude,
    double Longitude
    );
