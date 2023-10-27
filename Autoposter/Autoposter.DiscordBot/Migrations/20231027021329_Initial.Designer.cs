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
    [Migration("20231027021329_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discord_id");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_update_at");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("TagName")
                        .HasColumnType("text")
                        .HasColumnName("tag_name");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_posts");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_posts_user_id");

                    b.ToTable("posts", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.PostsServers", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid")
                        .HasColumnName("post_id");

                    b.Property<Guid>("ServerId")
                        .HasColumnType("uuid")
                        .HasColumnName("server_id");

                    b.HasKey("Id")
                        .HasName("pk_posts_servers");

                    b.HasIndex("PostId")
                        .HasDatabaseName("ix_posts_servers_post_id");

                    b.HasIndex("ServerId")
                        .HasDatabaseName("ix_posts_servers_server_id");

                    b.ToTable("posts_servers", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.PostsTags", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid")
                        .HasColumnName("post_id");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid")
                        .HasColumnName("tag_id");

                    b.HasKey("Id")
                        .HasName("pk_posts_tags");

                    b.HasIndex("PostId")
                        .HasDatabaseName("ix_posts_tags_post_id");

                    b.HasIndex("TagId")
                        .HasDatabaseName("ix_posts_tags_tag_id");

                    b.ToTable("posts_tags", (string)null);
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

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("DiscordId")
                        .HasColumnType("text")
                        .HasColumnName("discord_id");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("ServerName")
                        .HasColumnType("text")
                        .HasColumnName("server_name");

                    b.Property<string>("TagName")
                        .HasColumnType("text")
                        .HasColumnName("tag_name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.Post", b =>
                {
                    b.HasOne("Autoposter.DomainLayer.Entities.Autoposter.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_posts_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.PostsServers", b =>
                {
                    b.HasOne("Autoposter.DomainLayer.Entities.Autoposter.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_posts_servers_posts_post_id");

                    b.HasOne("Autoposter.DomainLayer.Entities.Autoposter.Server", "Server")
                        .WithMany()
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_posts_servers_servers_server_id");

                    b.Navigation("Post");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("Autoposter.DomainLayer.Entities.Autoposter.PostsTags", b =>
                {
                    b.HasOne("Autoposter.DomainLayer.Entities.Autoposter.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_posts_tags_posts_post_id");

                    b.HasOne("Autoposter.DomainLayer.Entities.Autoposter.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_posts_tags_tags_tag_id");

                    b.Navigation("Post");

                    b.Navigation("Tag");
                });
#pragma warning restore 612, 618
        }
    }
}
