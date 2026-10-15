namespace Portfolio.Application.Dto;

/// <summary>Acknowledgement returned after a contact submission is accepted.</summary>
public sealed record ContactMessageAcceptedDto(Guid Id, DateTime ReceivedAtUtc);
