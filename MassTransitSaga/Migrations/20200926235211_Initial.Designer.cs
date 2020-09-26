﻿// <auto-generated />
using System;
using MassTransitSaga.MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MassTransitSaga.Migrations
{
    [DbContext(typeof(VergunningStateDbContext))]
    [Migration("20200926235211_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MassTransitSaga.MassTransit.VergunningState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CurrentState")
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.HasKey("CorrelationId");

                    b.ToTable("VergunningState");
                });
#pragma warning restore 612, 618
        }
    }
}