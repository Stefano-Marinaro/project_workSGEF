using GoCare.Application.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Association> Associations => Set<Association>();
    public DbSet<CareGroup> CareGroups => Set<CareGroup>();
    public DbSet<CareGroupMembership> CareGroupMemberships => Set<CareGroupMembership>();
    public DbSet<SavedDestination> SavedDestinations => Set<SavedDestination>();
    public DbSet<TransportRequest> TransportRequests => Set<TransportRequest>();
    public DbSet<TransportRequestCandidate> TransportRequestCandidates => Set<TransportRequestCandidate>();
    public DbSet<TransportRequestRejection> TransportRequestRejections => Set<TransportRequestRejection>();
    public DbSet<Companion> Companions => Set<Companion>();
    public DbSet<TransportModificationRequest> TransportModificationRequests => Set<TransportModificationRequest>();
    public DbSet<TripStatusTransition> TripStatusTransitions => Set<TripStatusTransition>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ContactAccessLog> ContactAccessLogs => Set<ContactAccessLog>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //region = divisioni del codice per l'IDE, per semplifacere la lettura, visto che questo blocco avrà tutte le entità dell app

        #region "PERSON AND GROUPS"
        //PERSONS AND GROUPS

        modelBuilder.Entity<Person>(person =>
        {
            person.HasKey(p => p.Id);
            person.Property(p => p.Name).HasMaxLength(100);
            person.Property(p => p.Surname).HasMaxLength(100);
            person.Property(p => p.Email).HasMaxLength(255);
            person.Property(p => p.Phone).HasMaxLength(32);

            person.OwnsOne(p => p.HomeAddress, addr =>
            {
                addr.Property(a => a.Street).HasMaxLength(200);
                addr.Property(a => a.Number).HasMaxLength(20);
                addr.Property(a => a.PostalCode).HasMaxLength(10);
                addr.Property(a => a.City).HasMaxLength(120);
                addr.Property(a => a.Province).HasMaxLength(120);
            });
        });

        modelBuilder.Entity<Association>(association =>
        {
            // phones e CoveredProvinces non sono inclusi in lunghezza massima perchè sono List<string>
            association.HasKey(a => a.Id);
            association.Property(a => a.Name).HasMaxLength(200);
            association.Property(a => a.Email).HasMaxLength(255);
            association.Property(a => a.AvailabilityHours).HasMaxLength(200);

            association.OwnsOne(ad => ad.Headquarter, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);
            });
        });

        modelBuilder.Entity<CareGroup>(careGroup =>
        {
            careGroup.HasKey(c => c.Id);
            careGroup.Property(c => c.Name).HasMaxLength(200);
            careGroup.Property(c => c.Description).HasMaxLength(255);

            careGroup.HasOne<Person>()
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); // Rifiuta di eliminare Person finchè esiste CareGroup che lo referenzia. Utile per evitare eliminazioni a cascata. 
            
            careGroup.HasIndex(c => new { c.CreatedBy, c.Name }).IsUnique();
        });

        modelBuilder.Entity<CareGroupMembership>(membership =>
        {
            membership.HasKey(  m => new { m.CareGroupId, m.PersonId });

            membership.HasOne<CareGroup>()
                .WithMany()
                .HasForeignKey(m => m.CareGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            membership.HasOne<Person>()
                .WithMany()
                .HasForeignKey(m => m.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            membership.Property(m => m.InvitationEmail).HasMaxLength(255);
            membership.Property(m => m.InvitationToken).HasMaxLength(255);

            membership.HasIndex(m => m.InvitationToken).IsUnique();
        });

        modelBuilder.Entity<SavedDestination>(destination =>
        {
            destination.HasKey(d => d.Id);
            destination.Property(d => d.PlaceName).HasMaxLength(200);
            destination.Property(d => d.Note).HasMaxLength(300);

            destination.HasOne<Person>()
                .WithMany()
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            destination.OwnsOne(ad => ad.SavedAddress, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);
            });

        });
        #endregion

        #region "TRANSPORT"

        modelBuilder.Entity<TransportRequest>(transportReq =>
        {
            transportReq.HasKey(t => t.Id);
            transportReq.Property(t => t.ReferencePhone).HasMaxLength(32);
            transportReq.Property(t => t.ReferenceEmail).HasMaxLength(255);

            transportReq.HasOne<Person>()
                .WithMany()
                .HasForeignKey(t => t.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            transportReq.HasOne<Person>()
                .WithMany()
                .HasForeignKey(t => t.BeneficiaryId)
                .OnDelete(DeleteBehavior.Restrict);

            transportReq.HasOne<CareGroup>()
                .WithMany()
                .HasForeignKey(t => t.CareGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            transportReq.HasOne<Association>()
                .WithMany()
                .HasForeignKey(t => t.AssignedAssociationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            transportReq.OwnsOne(t => t.StartAddress, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);

                addr.HasIndex(ad => ad.Province); // indice perchè sarà frequente nelle query filtrare per provincia di provenienza. Quindi velocizzerà
            });

            transportReq.OwnsOne(t => t.EndAddress, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);
            });

            transportReq.OwnsOne(t => t.ReturnEndAddress, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);
            });

            transportReq.HasIndex(t => t.RequestStatus);
            
            transportReq.Property<uint>("xmin")     // questo blocco permette di evitare che sia cambiato il db in concerrenza (es: due associazioni provano a accettare 
                .HasColumnName("xmin")              // una stessa richiesta: se l'accetta prima una e l'altra pure in contemporanea, si generano errori. Con questo blocco
                .HasColumnType("xid")               // un id cambia a ogni operazione e permette lanciare un eccezione in caso non combaci)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TransportRequestCandidate>(candidate =>
        {
            candidate.HasKey(c => new { c.TransportRequestId, c.AssociationId });

            candidate.HasOne<Association>()
                .WithMany()
                .HasForeignKey(c => c.AssociationId)
                .OnDelete(DeleteBehavior.Restrict);

            candidate.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(c => c.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<TransportRequestRejection>(rejection =>
        {
            rejection.HasKey(r => r.Id);
            rejection.Property(r => r.Reason).HasMaxLength(300);

            rejection.HasOne<Association>()
                .WithMany()
                .HasForeignKey(r => r.AssociationId)
                .OnDelete(DeleteBehavior.Restrict);

            rejection.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(r => r.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Companion>(companion =>
        {
            companion.HasKey(c => c.Id);
            companion.Property(c => c.Name).HasMaxLength(200);
            companion.Property(c => c.Surname).HasMaxLength(200);
            companion.Property(c => c.Phone).HasMaxLength(32);
            companion.Property(c => c.Relationship).HasMaxLength(200);

            companion.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(c => c.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransportModificationRequest>(modification =>
        {
            modification.HasKey(m => m.Id);
            modification.Property(m => m.OutcomeMessage).HasMaxLength(300);

            modification.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(c => c.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TripStatusTransition>(transition =>
        {
            transition.HasKey(t => t.Id);
            transition.Property(t  => t.OperatorLabel).HasMaxLength(200);

            transition.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(t => t.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            transition.HasOne<Association>()
                .WithMany()
                .HasForeignKey(t => t.MadeByAssociationId)
                .OnDelete(DeleteBehavior.Restrict);

            transition.HasIndex(t => new { t.TransportRequestId, t.OccurredAt });
        });
        
        #endregion

        #region "NOTIFICATION"

        modelBuilder.Entity<Notification>(notification =>
        {
            notification.HasKey(n => n.Id);
            notification.Property(n => n.Title).HasMaxLength(200);
            notification.Property(n => n.Body).HasMaxLength(1000);

            notification.Property(n => n.Channels).HasConversion<int>(); // Channels è [Flags]: va salvato come int, non come stringa (sovrascrive la conversione globale)

            notification.HasIndex(n => new { n.SubjectType, n.SubjectId }); // SubjectId e RelatedEntityId sono Guid polimorfici (Person o Association / varie entità): nessuna FK
        });

        modelBuilder.Entity<ContactAccessLog>(contactLog =>
        {
            contactLog.HasKey(c => c.Id);

            contactLog.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(c => c.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            contactLog.HasOne<Association>()
                .WithMany()
                .HasForeignKey(c => c.AssociationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeviceToken>(deviceToken =>
        {
            deviceToken.HasKey(d => d.Id);

            deviceToken.HasIndex(d => new { d.SubjectType, d.SubjectId }); // SubjectId è un Guid polimorfico (Person o Association): nessuna FK
        });

        #endregion
    }
}

            

            
