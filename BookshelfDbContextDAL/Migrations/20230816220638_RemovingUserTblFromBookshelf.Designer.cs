﻿// <auto-generated />
using System;
using BookshelfDbContextDAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BookshelfDbContextDAL.Migrations
{
    [DbContext(typeof(BookshelfDbContext))]
    [Migration("20230816220638_RemovingUserTblFromBookshelf")]
    partial class RemovingUserTblFromBookshelf
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("BookshelfModels.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Authors")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("varchar(150)");

                    b.Property<string>("Comment")
                        .HasMaxLength(350)
                        .HasColumnType("varchar(350)");

                    b.Property<string>("Cover")
                        .HasMaxLength(2000)
                        .HasColumnType("varchar(2000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Genre")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("GoogleId")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<bool>("Inactive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Isbn")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int?>("Pages")
                        .HasMaxLength(3)
                        .HasColumnType("int");

                    b.Property<int?>("Score")
                        .HasMaxLength(1)
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasMaxLength(2)
                        .HasColumnType("int");

                    b.Property<string>("Subtitle")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("Volume")
                        .HasMaxLength(3)
                        .HasColumnType("int");

                    b.Property<int?>("Year")
                        .HasMaxLength(4)
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Book");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoric", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BookHistoricTypeId")
                        .HasColumnType("int");

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("BookHistoricTypeId");

                    b.HasIndex("BookId");

                    b.ToTable("BookHistoric");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoricItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BookHistoricId")
                        .HasColumnType("int");

                    b.Property<int>("BookHistoricItemFieldId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UpdatedFrom")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.Property<string>("UpdatedTo")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("BookHistoricId");

                    b.HasIndex("BookHistoricItemFieldId");

                    b.ToTable("BookHistoricItem");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoricItemField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("BookHistoricItemField");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoricType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("BookHistoricType");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoric", b =>
                {
                    b.HasOne("BookshelfModels.BookHistoricType", "BookHistoricType")
                        .WithMany()
                        .HasForeignKey("BookHistoricTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookshelfModels.Book", "Book")
                        .WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("BookHistoricType");
                });

            modelBuilder.Entity("BookshelfModels.BookHistoricItem", b =>
                {
                    b.HasOne("BookshelfModels.BookHistoric", "BookHistoric")
                        .WithMany()
                        .HasForeignKey("BookHistoricId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookshelfModels.BookHistoricItemField", "BookHistoricItemField")
                        .WithMany()
                        .HasForeignKey("BookHistoricItemFieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BookHistoric");

                    b.Navigation("BookHistoricItemField");
                });
#pragma warning restore 612, 618
        }
    }
}