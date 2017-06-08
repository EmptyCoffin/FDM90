USE [master]
GO
/****** Object:  Database [FDM90]    Script Date: 04/06/2017 20:14:00 ******/
CREATE DATABASE [FDM90]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'FDM90', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\FDM90.mdf' , SIZE = 16384KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'FDM90_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\FDM90_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [FDM90] SET COMPATIBILITY_LEVEL = 130
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [FDM90].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [FDM90] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [FDM90] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [FDM90] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [FDM90] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [FDM90] SET ARITHABORT OFF 
GO
ALTER DATABASE [FDM90] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [FDM90] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [FDM90] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [FDM90] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [FDM90] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [FDM90] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [FDM90] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [FDM90] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [FDM90] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [FDM90] SET  DISABLE_BROKER 
GO
ALTER DATABASE [FDM90] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [FDM90] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [FDM90] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [FDM90] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [FDM90] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [FDM90] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [FDM90] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [FDM90] SET RECOVERY FULL 
GO
ALTER DATABASE [FDM90] SET  MULTI_USER 
GO
ALTER DATABASE [FDM90] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [FDM90] SET DB_CHAINING OFF 
GO
ALTER DATABASE [FDM90] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [FDM90] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [FDM90] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [FDM90] SET QUERY_STORE = OFF
GO
USE [FDM90]
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [FDM90]
GO
/****** Object:  Table [dbo].[Facebook]    Script Date: 04/06/2017 20:14:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Facebook](
	[UserId] [uniqueidentifier] NOT NULL,
	[PageName] [varchar](255) NOT NULL,
	[PermanentAccessToken] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[PageName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Goals]    Script Date: 04/06/2017 20:14:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Goals](
	[UserId] [uniqueidentifier] NOT NULL,
	[GoalName] [varchar](255) NOT NULL,
	[WeekStart] [date] NOT NULL,
	[WeekEnd] [date] NOT NULL,
	[Targets] [varchar](max) NULL,
	[Progress] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[GoalName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Twitter]    Script Date: 04/06/2017 20:14:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Twitter](
	[UserId] [uniqueidentifier] NOT NULL,
	[AccessToken] [nvarchar](max) NOT NULL,
	[AccessTokenSecret] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[User]    Script Date: 04/06/2017 20:14:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[UserId] [uniqueidentifier] NOT NULL,
	[UserName] [varchar](255) NOT NULL,
	[Password] [varchar](max) NOT NULL,
	[EmailAddress] [varchar](255) NOT NULL,
	[Facebook] [bit] NOT NULL,
	[Twitter] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((0)) FOR [Twitter]
GO
ALTER TABLE [dbo].[Facebook]  WITH CHECK ADD  CONSTRAINT [FK_Facebook_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Facebook] CHECK CONSTRAINT [FK_Facebook_User]
GO
ALTER TABLE [dbo].[Goals]  WITH CHECK ADD  CONSTRAINT [FK_Goals_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Goals] CHECK CONSTRAINT [FK_Goals_User]
GO
USE [master]
GO
ALTER DATABASE [FDM90] SET  READ_WRITE 
GO
