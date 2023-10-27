﻿// <auto-generated />
using System;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Autoposter.DiscordBot.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231027200155_AddBotRoles1233")]
    partial class AddBotRoles1233
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.BotRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_bot_roles");

                    b.ToTable("bot_roles", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.BotSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("Interval")
                        .HasColumnType("integer")
                        .HasColumnName("interval");

                    b.HasKey("Id")
                        .HasName("pk_bot_settings");

                    b.ToTable("bot_settings", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Branch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<decimal>("BranchId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("branch_id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_branches");

                    b.ToTable("branches", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("BranchId")
                        .HasColumnType("text")
                        .HasColumnName("branch_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discord_id");

                    b.Property<string>("ImageUri")
                        .HasColumnType("text")
                        .HasColumnName("image_uri");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_at");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("ServerId")
                        .HasColumnType("text")
                        .HasColumnName("server_id");

                    b.Property<string>("TagName")
                        .HasColumnType("text")
                        .HasColumnName("tag_name");

                    b.HasKey("Id")
                        .HasName("pk_posts");

                    b.ToTable("posts", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.HasKey("Id")
                        .HasName("pk_servers");

                    b.ToTable("servers", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.ToTable("tags", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
