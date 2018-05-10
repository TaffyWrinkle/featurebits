﻿// <auto-generated />
using FeatureBitsData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using FeatureBits.Data;

namespace FeatureBitsData.Migrations
{
    [DbContext(typeof(FeatureBitsEfDbContext))]
    [Migration("20180424182241_Initial-Create")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FeatureBitsData.FeatureBitDefinition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllowedUsers")
                        .HasMaxLength(2048);

                    b.Property<string>("CreatedByUser")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("ExcludedEnvironments")
                        .HasMaxLength(300);

                    b.Property<string>("LastModifiedByUser")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<DateTime>("LastModifiedDateTime");

                    b.Property<int>("MinimumAllowedPermissionLevel");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<bool>("OnOff");

                    b.HasKey("Id");

                    b.ToTable("FeatureBitDefinitions");
                });
#pragma warning restore 612, 618
        }
    }
}
