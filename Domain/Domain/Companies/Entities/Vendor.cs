using Domain.Companies.ValueObjects;

namespace Domain.Companies.Entities;

public class Vendor {

    public Guid Id { get; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public string Phone { get; set; }
    public byte[] Logo { get; set; } = Array.Empty<byte>();
    public ExportProfile ExportProfile { get; set; }
    public ReleaseProfile ReleaseProfile { get; set; }

    internal Vendor(Guid id, string name, Address address, string phone, byte[] logo, ExportProfile exportProfile, ReleaseProfile releaseProfile) {
        Id = id;
        Name = name;
        Address = address;
        Phone = phone;
        Logo = logo;
        ExportProfile = exportProfile;
        ReleaseProfile = releaseProfile;
    }

    public static Vendor Create(string name, Address address, string phone, byte[] logo, ExportProfile exportProfile, ReleaseProfile releaseProfile)
        => new(Guid.NewGuid(), name, address, phone, logo, exportProfile, releaseProfile);

}