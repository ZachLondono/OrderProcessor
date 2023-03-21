using ApplicationCore.Features.Companies.Contracts.ValueObjects;

namespace ApplicationCore.Features.Companies.Contracts.Entities;

public class Vendor {

    public Guid Id { get; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public string Phone { get; set; }
    public byte[] Logo { get; set; } = Array.Empty<byte>();
    public ExportProfile ExportProfile { get; set; }
    public ReleaseProfile ReleaseProfile { get; set; }
    public EmailSender EmailSender { get; set; }

    internal Vendor(Guid id, string name, Address address, string phone, byte[] logo, ExportProfile exportProfile, ReleaseProfile releaseProfile, EmailSender emailSender) {
        Id = id;
        Name = name;
        Address = address;
        Phone = phone;
        Logo = logo;
        ExportProfile = exportProfile;
        ReleaseProfile = releaseProfile;
        EmailSender = emailSender;
    }

    public static Vendor Create(string name, Address address, string phone, byte[] logo, ExportProfile exportProfile, ReleaseProfile releaseProfile, EmailSender emailSender)
        => new(Guid.NewGuid(), name, address, phone, logo, exportProfile, releaseProfile, emailSender);

}