﻿// <auto-generated />
using System;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    [DbContext(typeof(FootballContext))]
    partial class FootballContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.13");

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord", b =>
                {
                    b.Property<int>("GameID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AwayTeamID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AwayTeamStrengthsAtKickoffJSON")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("GameComplete")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HomeTeamID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HomeTeamStrengthsAtKickoffJSON")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("KickoffTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("SeasonRecordID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StadiumID")
                        .HasColumnType("INTEGER");

                    b.Property<double>("TemperatureAtKickoff")
                        .HasColumnType("REAL");

                    b.Property<int>("WeatherAtKickoff")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WeekNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("GameID");

                    b.HasIndex("AwayTeamID");

                    b.HasIndex("HomeTeamID");

                    b.HasIndex("SeasonRecordID");

                    b.HasIndex("StadiumID");

                    b.ToTable("GameRecords");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.QuarterBoxScore", b =>
                {
                    b.Property<int>("QuarterBoxScoreID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameRecordID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("QuarterNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Team")
                        .HasColumnType("INTEGER");

                    b.HasKey("QuarterBoxScoreID");

                    b.HasIndex("GameRecordID");

                    b.ToTable("QuarterBoxScores");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.SeasonRecord", b =>
                {
                    b.Property<int>("SeasonRecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SeasonComplete")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("SeasonRecordID");

                    b.ToTable("SeasonRecords");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.SimulatorSettings", b =>
                {
                    b.Property<int>("SimulatorSettingsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SeedDataInitialized")
                        .HasColumnType("INTEGER");

                    b.HasKey("SimulatorSettingsID");

                    b.ToTable("SimulatorSettings");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.Stadium", b =>
                {
                    b.Property<int>("StadiumID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AverageTemperatures")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("AverageWindSpeed")
                        .HasColumnType("REAL");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("TotalPrecipitationOverSeason")
                        .HasColumnType("REAL");

                    b.HasKey("StadiumID");

                    b.ToTable("Stadium");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.Team", b =>
                {
                    b.Property<int>("TeamID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Abbreviation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CityName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("ClockManagementStrength")
                        .HasColumnType("REAL");

                    b.Property<int>("Conference")
                        .HasColumnType("INTEGER");

                    b.Property<double>("DefensiveLineStrength")
                        .HasColumnType("REAL");

                    b.Property<int>("Disposition")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Division")
                        .HasColumnType("INTEGER");

                    b.Property<double>("FieldGoalStrength")
                        .HasColumnType("REAL");

                    b.Property<int>("HomeStadiumID")
                        .HasColumnType("INTEGER");

                    b.Property<double>("KickDefenseStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("KickReturnStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("KickingStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("OffensiveLineStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("PassingDefenseStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("PassingOffenseStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("RunningDefenseStrength")
                        .HasColumnType("REAL");

                    b.Property<double>("RunningOffenseStrength")
                        .HasColumnType("REAL");

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TeamID");

                    b.HasIndex("HomeStadiumID");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.TeamDriveRecord", b =>
                {
                    b.Property<int>("TeamDriveRecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DriveDurationSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DriveStartTimeSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameRecordID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NetYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("QuarterNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Result")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StartingFieldPosition")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Team")
                        .HasColumnType("INTEGER");

                    b.HasKey("TeamDriveRecordID");

                    b.HasIndex("GameRecordID");

                    b.ToTable("TeamDriveRecords");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.TeamGameRecord", b =>
                {
                    b.Property<int>("TeamGameRecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AverageFourthDownDistance")
                        .HasColumnType("REAL");

                    b.Property<double>("AverageThirdDownDistance")
                        .HasColumnType("REAL");

                    b.Property<int>("ExtraPointAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExtraPointAttemptsMade")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FieldGoalAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FieldGoalsMade")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FirstDowns")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FourthDownConversionAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FourthDownConversions")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Fumbles")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FumblesLost")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameRecordID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PassAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PassCompletions")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PassInterceptions")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PassTouchdowns")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PassYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Penalties")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PenaltyYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PuntYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Punts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RushAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RushTouchdowns")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RushYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SackYards")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Sacks")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Team")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ThirdDownConversionAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ThirdDownConversions")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TimeOfPossessionSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TwoPointConversionAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TwoPointConversionAttemptsMade")
                        .HasColumnType("INTEGER");

                    b.HasKey("TeamGameRecordID");

                    b.HasIndex("GameRecordID");

                    b.ToTable("TeamGameRecords");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord", b =>
                {
                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.SeasonRecord", "SeasonRecord")
                        .WithMany("GameRecords")
                        .HasForeignKey("SeasonRecordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.Stadium", "Stadium")
                        .WithMany()
                        .HasForeignKey("StadiumID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AwayTeam");

                    b.Navigation("HomeTeam");

                    b.Navigation("SeasonRecord");

                    b.Navigation("Stadium");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.QuarterBoxScore", b =>
                {
                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord", "GameRecord")
                        .WithMany()
                        .HasForeignKey("GameRecordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameRecord");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.Team", b =>
                {
                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.Stadium", "HomeStadium")
                        .WithMany()
                        .HasForeignKey("HomeStadiumID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HomeStadium");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.TeamDriveRecord", b =>
                {
                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord", "GameRecord")
                        .WithMany()
                        .HasForeignKey("GameRecordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameRecord");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.TeamGameRecord", b =>
                {
                    b.HasOne("Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord", "GameRecord")
                        .WithMany()
                        .HasForeignKey("GameRecordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GameRecord");
                });

            modelBuilder.Entity("Celarix.JustForFun.FootballSimulator.Data.Models.SeasonRecord", b =>
                {
                    b.Navigation("GameRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
