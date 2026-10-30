namespace GoCare.Models.Domain;

// Come Address, ma con le coordinate obbligatorie: usato solo per gli indirizzi
// di un TransportRequest (partenza/destinazione/ritorno), dove servono per
// calcolare il percorso stradale e quindi il prezzo. Gli altri indirizzi
// dell'app (profilo, assistito, sede associazione, destinazioni salvate)
// continuano a usare Address, senza coordinate obbligatorie.
public sealed record TripAddress(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province,
    double Latitude,
    double Longitude
    );
