﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Service_Billing.Data;

#nullable disable

namespace Service_Billing.Migrations
{
    [DbContext(typeof(ServiceBillingContext))]
    partial class ServiceBillingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Service_Billing.Models.Bill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("BillingCycle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ClientAccountId")
                        .HasColumnType("int");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CurrentFiscalPeriodId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("DateCreated")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("EndDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("IdirOrUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Quantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ServiceCategoryId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("StartDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("TicketNumberAndRequester")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("ticketNumberAndRequesterName");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClientAccountId");

                    b.HasIndex("CurrentFiscalPeriodId");

                    b.HasIndex("ServiceCategoryId");

                    b.ToTable("Bills", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.BusinessArea", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Acronym")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("BusAreas", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.ClientAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Approver")
                        .HasColumnType("nvarchar(max)");

                    b.Property<short?>("ClientNumber")
                        .HasColumnType("smallint");

                    b.Property<string>("ExpenseAuthorityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FinancialContact")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsApprovedByEA")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OrganizationId")
                        .HasColumnType("int");

                    b.Property<string>("PrimaryContact")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResponsibilityCentre")
                        .HasColumnType("nvarchar(max)");

                    b.Property<short?>("STOB")
                        .HasColumnType("smallint");

                    b.Property<int?>("ServiceLine")
                        .HasColumnType("int");

                    b.Property<string>("ServicesEnabled")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ClientAccounts", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<int?>("ClientAccountId")
                        .HasColumnType("int");

                    b.Property<string>("ContactType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClientAccountId");

                    b.HasIndex("PersonId");

                    b.ToTable("Contact", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.FiscalHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BillId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PeriodId")
                        .HasColumnType("int");

                    b.Property<decimal?>("QuantityAtFiscal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("UnitPriceAtFiscal")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("BillId");

                    b.HasIndex("PeriodId");

                    b.ToTable("FiscalHistory", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.FiscalPeriod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Period")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("FiscalPeriods", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.Ministry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Acronym")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Ministries", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Mail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("People", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.ServiceCategory", b =>
                {
                    b.Property<int>("ServiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ServiceId"));

                    b.Property<int>("BusAreaId")
                        .HasColumnType("int");

                    b.Property<string>("Costs")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServiceOwner")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UOM")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ServiceId");

                    b.HasIndex("BusAreaId");

                    b.ToTable("ServiceCategories", (string)null);
                });

            modelBuilder.Entity("Service_Billing.Models.Bill", b =>
                {
                    b.HasOne("Service_Billing.Models.ClientAccount", "ClientAccount")
                        .WithMany("Bills")
                        .HasForeignKey("ClientAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Service_Billing.Models.FiscalPeriod", "MostRecentActiveFiscalPeriod")
                        .WithMany()
                        .HasForeignKey("CurrentFiscalPeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Service_Billing.Models.ServiceCategory", "ServiceCategory")
                        .WithMany()
                        .HasForeignKey("ServiceCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClientAccount");

                    b.Navigation("MostRecentActiveFiscalPeriod");

                    b.Navigation("ServiceCategory");
                });

            modelBuilder.Entity("Service_Billing.Models.Contact", b =>
                {
                    b.HasOne("Service_Billing.Models.ClientAccount", null)
                        .WithMany("Contacts")
                        .HasForeignKey("ClientAccountId");

                    b.HasOne("Service_Billing.Models.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Service_Billing.Models.FiscalHistory", b =>
                {
                    b.HasOne("Service_Billing.Models.Bill", "Bill")
                        .WithMany("PreviousFiscalRecords")
                        .HasForeignKey("BillId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Service_Billing.Models.FiscalPeriod", "FiscalPeriod")
                        .WithMany()
                        .HasForeignKey("PeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bill");

                    b.Navigation("FiscalPeriod");
                });

            modelBuilder.Entity("Service_Billing.Models.ServiceCategory", b =>
                {
                    b.HasOne("Service_Billing.Models.BusinessArea", "BusArea")
                        .WithMany("Categories")
                        .HasForeignKey("BusAreaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BusArea");
                });

            modelBuilder.Entity("Service_Billing.Models.Bill", b =>
                {
                    b.Navigation("PreviousFiscalRecords");
                });

            modelBuilder.Entity("Service_Billing.Models.BusinessArea", b =>
                {
                    b.Navigation("Categories");
                });

            modelBuilder.Entity("Service_Billing.Models.ClientAccount", b =>
                {
                    b.Navigation("Bills");

                    b.Navigation("Contacts");
                });
#pragma warning restore 612, 618
        }
    }
}
