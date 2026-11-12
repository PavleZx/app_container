using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RPPP_WebApp.Model;
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Model;

public partial class Rppp08Context : DbContext
{
     private readonly IConfiguration _configuration;

    // Add a constructor that takes IConfiguration
    public Rppp08Context(DbContextOptions<Rppp08Context> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Now you can use _configuration to get the connection string
        var connectionString = _configuration.GetConnectionString("RPPP08");
        optionsBuilder.UseSqlServer(connectionString);
    }

    public virtual DbSet<AnimalName> AnimalNames { get; set; }

    public virtual DbSet<Enclosure> Enclosures { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<IndividualAnimals> IndividualAnimals { get; set; }

    public virtual DbSet<AnimalType> AnimalTypes { get; set; }
    public virtual DbSet<Worker> Workers { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Competence> Competences { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }
    public virtual DbSet<IncidentReport> IncidentReports { get; set; }
    public virtual DbSet<IncidentType> IncidentTypes { get; set; }
    public virtual DbSet<Affects> Affects { get; set; }

    public virtual DbSet<TourType> TourTypes { get; set; }

    public virtual DbSet<VisitGroup> VisitGroups { get; set; }

    public virtual DbSet<GroupGuide> GroupGuides { get; set; }

    public virtual DbSet<RPPP_WebApp.Model.Task> Tasks { get; set; }
    // Task is also a name for an implementation of a class in Threading

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<Price> Prices { get; set; }

    public DbSet<TaskStatus> TaskStatuses { get; set; }

    public virtual DbSet<LogEntry> Logs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimalName>(entity =>
        {
            entity.ToTable("AnimalName");

            entity.Property(e => e.NameID)
                .ValueGeneratedOnAdd()
                .HasColumnName("nameID");
            entity.Property(e => e.LatinName).HasMaxLength(50);
            entity.Property(e => e.CroatianName).HasMaxLength(50);
        });

        modelBuilder.Entity<AnimalType>(entity =>
        {
            entity.ToTable("AnimalType");

            entity.Property(e => e.AnimalTypeID)
                .ValueGeneratedOnAdd()
                .HasColumnName("AnimalTypeID");
            entity.Property(e => e.AnimalTypes).HasMaxLength(50).HasColumnName("AnimalType");
        });

        modelBuilder.Entity<Enclosure>(entity =>
        {
            entity.ToTable("Enclosure");

            entity.Property(e => e.EnclosureID)
                .ValueGeneratedOnAdd()
                .HasColumnName("enclosureID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Dimensions).HasMaxLength(50);
            entity.Property(e => e.Change).HasMaxLength(50);
            entity.Property(e => e.Lighting).HasMaxLength(50);
            entity.Property(e => e.Shading).HasMaxLength(50);
            entity.Property(e => e.LocationID).HasColumnName("locationID");

            entity.HasOne(d => d.Location).WithMany(p => p.Enclosures)
                .HasForeignKey(d => d.LocationID)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property(e => e.AnimalTypeID).HasColumnName("animalTypeID");

            entity.HasOne(d => d.AnimalType).WithMany(p => p.Enclosures)
                .HasForeignKey(d => d.AnimalTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");

            entity.Property(e => e.LocationID)
                .ValueGeneratedOnAdd()   
                .HasColumnName("locationID");
            entity.Property(e => e.LocationName).HasMaxLength(50);
        });

        modelBuilder.Entity<IndividualAnimals>(entity =>
        {
            entity.ToTable("IndividualAnimals");

            entity.Property(e => e.AnimalID)
                .ValueGeneratedOnAdd()
                .HasColumnName("animalID");
            entity.Property(e => e.SerialNum).HasMaxLength(50);
            entity.Property(e => e.Nickname).HasMaxLength(50);
            entity.Property(e => e.Acquisition).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.EnclosureID).HasColumnName("enclosureID");

            entity.HasOne(d => d.Enclosure).WithMany(p => p.IndividualAnimals)
                .HasForeignKey(d => d.EnclosureID)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property(e => e.NameID).HasColumnName("nameID");

            entity.HasOne(d => d.AnimalName).WithMany(p => p.IndividualAnimals)
                .HasForeignKey(d => d.NameID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.ToTable("Availability");

            entity.Property(e => e.AvailabilityId)
                .ValueGeneratedNever()
                .HasColumnName("availabilityID");
            entity.Property(e => e.AvailabilityDate).HasColumnName("availabilityDate");
            entity.Property(e => e.WorkHours)
                .HasColumnType("datetime")
                .HasColumnName("workHours");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.ToTable("Certificate");

            entity.Property(e => e.CertificateId).HasColumnName("certificateID");
            entity.Property(e => e.CertificateName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("certificateName");
        });

        modelBuilder.Entity<Competence>(entity =>
        {
            entity.ToTable("Competence");

            entity.Property(e => e.CompetenceId).HasColumnName("competenceID");
            entity.Property(e => e.CertificateId).HasColumnName("certificateID");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiryDate");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.WorkerId).HasColumnName("workerID");

            entity.HasOne(d => d.Certificate).WithMany(p => p.Competences)
                .HasForeignKey(d => d.CertificateId)
                .HasConstraintName("FK_Competence_Certificate");

            entity.HasOne(d => d.Worker).WithMany(p => p.Competences)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK_Competence_Worker");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.ToTable("Worker");

            entity.Property(e => e.WorkerId).HasColumnName("workerID");
            entity.Property(e => e.AvailabilityId).HasColumnName("availabilityID");
            entity.Property(e => e.Email)
                .HasMaxLength(25)
                .IsFixedLength()
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .IsRequired()
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("surname");

            entity.HasOne(d => d.Availability).WithMany(p => p.Workers)
                .HasForeignKey(d => d.AvailabilityId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Worker_Availability");
        });

        modelBuilder.Entity<TourType>(entity =>
        {
            entity.HasKey(e => e.TourId);

            entity.ToTable("TourType");

            entity.Property(e => e.TourId).HasColumnName("tourID");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.TourName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("tourName");
        });

        modelBuilder.Entity<VisitGroup>(entity =>
        {
            entity.ToTable("VisitGroup");

            entity.Property(e => e.VisitGroupId).HasColumnName("visitGroupID");
            entity.Property(e => e.Approval)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("approval");
            entity.Property(e => e.DateOfVisiting).HasColumnName("dateOfVisiting");
            entity.Property(e => e.GroupName)
                .IsRequired()
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("groupName");
            entity.Property(e => e.TourId).HasColumnName("tourID");
            entity.Property(e => e.VisitorCount).HasColumnName("visitorCount");

            entity.HasOne(d => d.Tour).WithMany(p => p.VisitGroups)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_VisitGroup_TourType"); 
        });

        modelBuilder.Entity<GroupGuide>(entity =>
        {
            entity.ToTable("GroupGuide");

            entity.Property(e => e.GroupGuideId).HasColumnName("GroupGuideID");
            entity.Property(e => e.VisitGroupId).HasColumnName("visitGroupID");
            entity.Property(e => e.WorkerId).HasColumnName("workerID");

            entity.HasOne(d => d.VisitGroup).WithMany(p => p.GroupGuides)
                .HasForeignKey(d => d.VisitGroupId)
                .HasConstraintName("FK_GroupGuide_VisitGroup");

            entity.HasOne(d => d.Worker).WithMany(p => p.GroupGuides)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK_GroupGuide_Worker");
        });

        modelBuilder.Entity<IncidentType>(entity =>
        {
            entity.ToTable("IncidentType");

            entity.Property(e => e.IncidentTypeID)
                .HasColumnName("incidentTypeID");

            entity.Property(e => e.incidentType)
                .IsRequired()
                .HasMaxLength(15)
                .HasColumnName("incidentType");

            entity.Property(e => e.Description)
                .HasMaxLength(30)
                .HasColumnName("description");
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.ToTable("IncidentReport");
            entity.Property(e => e.ReportID)
                .ValueGeneratedNever()
                .HasColumnName("reportID");

            entity.Property(e => e.DateReport)
                .HasColumnType("date")
                .HasColumnName("dateReport");

            entity.Property(e => e.DateSolved)
                .HasColumnType("date")
                .HasColumnName("dateSolved");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("status");

            entity.Property(e => e.TypeID)
                .HasColumnName("typeID");

            entity.HasOne(d => d.IncidentType)
                .WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentReport_IncidentType");
        });

        modelBuilder.Entity<Affects>(entity =>
        {
            entity.ToTable("Affects");
            entity.Property(e => e.affectsID)
                .ValueGeneratedNever(); 
            entity.HasOne(d => d.IncidentReport)
                .WithMany(p => p.Affects)
                .HasForeignKey(d => d.reportID);
        });

        modelBuilder.Entity<RPPP_WebApp.Model.Task>(entity =>
        {
            entity.ToTable("Task");

            entity.Property(e => e.TaskID)
                .ValueGeneratedOnAdd()  
                .HasColumnName("taskID");

            entity.Property(e => e.TimeStart).HasColumnName("timeStart");
            entity.Property(e => e.TimeEnd).HasColumnName("timeEnd");
            entity.Property(e => e.Notes).HasMaxLength(40);

            entity.Property(e => e.EnclosureID).HasColumnName("enclosureID");
            entity.Property(e => e.TypeID).HasColumnName("typeID");
            entity.Property(e => e.StatusID).HasColumnName("statusID");

            entity.HasOne(d => d.Enclosure)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EnclosureID);

            entity.HasOne(d => d.TaskType)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TypeID);

            entity.HasOne(d => d.TaskStatus)
                .WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusID);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expenses");

            entity.Property(e => e.Type).HasMaxLength(25);
            entity.Property(e => e.ArrivalDate).HasColumnName("arrivalDate");
            entity.Property(e => e.Description).HasMaxLength(50);


            // entity.HasOne(d => d.Task)
            //     .WithMany(p => p.Expenses)
            //     .HasForeignKey(d => d.TaskID);

            // entity.HasOne(d => d.Price)
            //     .WithMany(p => p.Expenses)
            //     .HasForeignKey(d => d.PriceID);
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.ToTable("TaskType");

            entity.Property(e => e.TaskTypeID)
                .HasColumnName("taskTypeID");

            entity.Property(e => e.Name)
                .HasColumnName("taskType")
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(30)
                .IsFixedLength();
        });


        modelBuilder.Entity<Price>(entity =>
        {
            entity.ToTable("Price");

            entity.HasKey(e => e.PriceID);

            entity.Property(e => e.PriceID)
                .HasColumnName("priceID");

            entity.Property(e => e.ItemName)
                .HasColumnName("itemName");

            entity.Property(e => e.PriceValue)
                .HasColumnName("price");
        });

        modelBuilder.Entity<TaskStatus>(entity =>
        {
            entity.ToTable("TaskStatus");

            entity.Property(e => e.TaskStatusID)
                .HasColumnName("taskStatusID");

            entity.Property(e => e.Status)
                .HasColumnName("taskStatus")
                .HasMaxLength(15)
                .IsFixedLength();
        });







        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
