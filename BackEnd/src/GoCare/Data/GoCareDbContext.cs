using GoCare.Models.Auth;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Data;

public sealed class GoCareDbContext(DbContextOptions<GoCareDbContext> options) : DbContext(options)
{
    // Auth
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<EmailChangeToken> EmailChangeTokens => Set<EmailChangeToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Transports
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Association> Associations => Set<Association>();
    public DbSet<AssistedPerson> AssistedPeople => Set<AssistedPerson>();
    public DbSet<CaregiverAssistedLink> CaregiverAssistedLinks => Set<CaregiverAssistedLink>();
    public DbSet<SavedDestination> SavedDestinations => Set<SavedDestination>();
    public DbSet<TransportRequest> TransportRequests => Set<TransportRequest>();
    public DbSet<TransportRequestCandidate> TransportRequestCandidates => Set<TransportRequestCandidate>();
    public DbSet<TransportRequestRejection> TransportRequestRejections => Set<TransportRequestRejection>();
    public DbSet<Companion> Companions => Set<Companion>();
    public DbSet<TripStatusTransition> TripStatusTransitions => Set<TripStatusTransition>();
    public DbSet<TripOperatorLink> TripOperatorLinks => Set<TripOperatorLink>();
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

        #region "AUTH"

        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(a => a.Id);
            account.Property(a => a.Email).HasMaxLength(255);
            account.Property(a => a.Role);       // proprietà immutabile: EF Core non la include per convenzione, va dichiarata a mano
            account.Property(a => a.CreatedAt);  // idem
            account.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<EmailVerificationToken>(token =>
        {
            token.HasKey(t => t.Id);
            token.Property(t => t.Token).HasMaxLength(255);
            token.Property(t => t.ExpiresAt);

            token.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            token.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<EmailChangeToken>(token =>
        {
            token.HasKey(t => t.Id);
            token.Property(t => t.NewEmail).HasMaxLength(255);
            token.Property(t => t.Token).HasMaxLength(255);
            token.Property(t => t.ExpiresAt);

            token.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            token.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<PasswordResetToken>(passToken =>
        {
            passToken.HasKey(p => p.Id);
            passToken.Property(t => t.Token).HasMaxLength(255);
            passToken.Property(t => t.ExpiresAt);

            passToken.HasOne<Account>()
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            passToken.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.HasKey(p => p.Id);
            refreshToken.Property(t => t.Token).HasMaxLength(255);
            refreshToken.Property(t => t.ExpiresAt);
            refreshToken.Property(t => t.CreatedAt);

            refreshToken.HasOne<Account>()
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            refreshToken.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<FailedLoginAttempt>(attempt =>
        {
            attempt.HasKey(a => a.Id);
            attempt.Property(a => a.Email).HasMaxLength(255);
            attempt.Property(a => a.IpAddress).HasMaxLength(45);

            attempt.HasIndex(a => new { a.Email, a.AttemptedAt });
        });

        #endregion

        #region "PERSON AND ASSISTED"

        modelBuilder.Entity<Person>(person =>
        {
            person.HasKey(p => p.Id);
            person.Property(p => p.Name).HasMaxLength(100);
            person.Property(p => p.Surname).HasMaxLength(100);
            person.Property(p => p.Email).HasMaxLength(255);
            person.Property(p => p.Phone).HasMaxLength(32);

            // Person.Id è sia la sua PK sia la FK verso Account.Id (PK condivisa): ora che
            // Account e Person vivono nello stesso database, il vincolo può essere reale,
            // non solo una convenzione applicativa. Cascade: eliminato l'Account, sparisce il Person.
            person.HasOne<Account>()
                .WithOne()
                .HasForeignKey<Person>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Association.Id è sia la sua PK sia la FK verso Account.Id (PK condivisa), stesso motivo del Person sopra.
            association.HasOne<Account>()
                .WithOne()
                .HasForeignKey<Association>(a => a.Id)
                .OnDelete(DeleteBehavior.Cascade);

            association.OwnsOne(ad => ad.Headquarter, addr =>
            {
                addr.Property(ad => ad.Street).HasMaxLength(200);
                addr.Property(ad => ad.Number).HasMaxLength(20);
                addr.Property(ad => ad.PostalCode).HasMaxLength(10);
                addr.Property(ad => ad.City).HasMaxLength(120);
                addr.Property(ad => ad.Province).HasMaxLength(120);
            });
        });

        modelBuilder.Entity<AssistedPerson>(assisted =>
        {
            assisted.HasKey(a => a.Id);
            assisted.Property(a => a.Name).HasMaxLength(100);
            assisted.Property(a => a.Surname).HasMaxLength(100);
            assisted.Property(a => a.Phone).HasMaxLength(32);

            assisted.HasOne<Person>()
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            assisted.OwnsOne(a => a.HomeAddress, addr =>
            {
                addr.Property(a => a.Street).HasMaxLength(200);
                addr.Property(a => a.Number).HasMaxLength(20);
                addr.Property(a => a.PostalCode).HasMaxLength(10);
                addr.Property(a => a.City).HasMaxLength(120);
                addr.Property(a => a.Province).HasMaxLength(120);
            });
        });

        modelBuilder.Entity<CaregiverAssistedLink>(link =>
        {
            link.HasKey(l => new { l.CaregiverId, l.AssistedPersonId });

            link.HasOne<Person>()
                .WithMany()
                .HasForeignKey(l => l.CaregiverId)
                .OnDelete(DeleteBehavior.Restrict);

            link.HasOne<AssistedPerson>()
                .WithMany()
                .HasForeignKey(l => l.AssistedPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            link.Property(l => l.CreatedAt);

            link.HasIndex(l => l.AssistedPersonId); // query "tutti i caregiver di questo assistito"
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

            transportReq.HasOne<AssistedPerson>()
                .WithMany()
                .HasForeignKey(t => t.BeneficiaryId)
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

            transportReq.Property(t => t.CreatedAt);

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
            rejection.Property(r => r.Kind);
            rejection.Property(r => r.RejectedAt);

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

        modelBuilder.Entity<TripStatusTransition>(transition =>
        {
            transition.HasKey(t => t.Id);
            transition.Property(t => t.OperatorLabel).HasMaxLength(200);
            transition.Property(t => t.Status);

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

        modelBuilder.Entity<TripOperatorLink>(opLink =>
        {
            opLink.HasKey(o => o.Id);
            opLink.Property(o => o.Token).HasMaxLength(255);
            opLink.Property(o => o.ExpiresAt);
            opLink.Property(o => o.CreatedAt);

            opLink.HasOne<TransportRequest>()
                .WithMany()
                .HasForeignKey(o => o.TransportRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            opLink.HasIndex(o => o.Token).IsUnique();
            opLink.HasIndex(o => o.TransportRequestId); // per trovare i link (attivi e passati) di un trasporto
        });

        #endregion

        #region "NOTIFICATION"

        modelBuilder.Entity<Notification>(notification =>
        {
            notification.HasKey(n => n.Id);
            notification.Property(n => n.Title).HasMaxLength(200);
            notification.Property(n => n.Body).HasMaxLength(1000);

            notification.Property(n => n.Channels).HasConversion<int>(); // Channels è [Flags]: va salvato come int, non come stringa (sovrascrive la conversione globale)
            notification.Property(n => n.Type);
            notification.Property(n => n.CreatedAt);

            notification.HasIndex(n => new { n.SubjectType, n.SubjectId }); // SubjectId e RelatedEntityId sono Guid polimorfici (Person o Association / varie entità): nessuna FK
        });

        modelBuilder.Entity<ContactAccessLog>(contactLog =>
        {
            contactLog.HasKey(c => c.Id);
            contactLog.Property(c => c.DataKind);
            contactLog.Property(c => c.AccessedAt);

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
            deviceToken.Property(d => d.PushToken);
            deviceToken.Property(d => d.Platform);
            deviceToken.Property(d => d.CreatedAt);

            deviceToken.HasIndex(d => new { d.SubjectType, d.SubjectId }); // SubjectId è un Guid polimorfico (Person o Association): nessuna FK
        });

        #endregion
    }
}
