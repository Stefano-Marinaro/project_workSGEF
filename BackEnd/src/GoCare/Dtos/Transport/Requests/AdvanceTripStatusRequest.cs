using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Requests;

public sealed record AdvanceTripStatusRequest(ETripOperatorAction Action, string OperatorLabel);
