USE [master]
GO
/****** Object:  Database [FDM90]    Script Date: 26/08/2017 20:07:32 ******/
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
/****** Object:  Login [NT SERVICE\Winmgmt]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT SERVICE\Winmgmt] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [NT SERVICE\SQLWriter]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT SERVICE\SQLWriter] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [NT SERVICE\SQLTELEMETRY$SQLEXPRESS]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT SERVICE\SQLTELEMETRY$SQLEXPRESS] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [NT SERVICE\SQLAgent$SQLEXPRESS]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT SERVICE\SQLAgent$SQLEXPRESS] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [NT Service\MSSQL$SQLEXPRESS]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT Service\MSSQL$SQLEXPRESS] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [NT AUTHORITY\SYSTEM]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/****** Object:  Login [hp\owner]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [hp\owner] FROM WINDOWS WITH DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english]
GO
/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [FDM90Login]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [FDM90Login] WITH PASSWORD=N'password', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
GO
/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [##MS_PolicyTsqlExecutionLogin##]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [##MS_PolicyTsqlExecutionLogin##] WITH PASSWORD=N'gtfEzpN+IlRS78tQ9iaiEWQNYnyHcKp63nLLPvLoWNw=', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO
ALTER LOGIN [##MS_PolicyTsqlExecutionLogin##] DISABLE
GO
/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [##MS_PolicyEventProcessingLogin##]    Script Date: 26/08/2017 20:07:33 ******/
CREATE LOGIN [##MS_PolicyEventProcessingLogin##] WITH PASSWORD=N'sa9xT5NT+3hjvQKwyjIjxCzo8fKnGkTNIr3yrkP/qZM=', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO
ALTER LOGIN [##MS_PolicyEventProcessingLogin##] DISABLE
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [NT SERVICE\Winmgmt]
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [NT SERVICE\SQLWriter]
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [NT SERVICE\SQLAgent$SQLEXPRESS]
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [NT Service\MSSQL$SQLEXPRESS]
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [hp\owner]
GO
USE [FDM90]
GO
/****** Object:  User [FDM90Login]    Script Date: 26/08/2017 20:07:33 ******/
CREATE USER [FDM90Login] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_accessadmin] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_securityadmin] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_backupoperator] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_datareader] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_denydatareader] ADD MEMBER [FDM90Login]
GO
ALTER ROLE [db_denydatawriter] ADD MEMBER [FDM90Login]
GO
/****** Object:  Table [dbo].[Campaigns]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Campaigns](
	[UserId] [uniqueidentifier] NOT NULL,
	[CampaignName] [varchar](255) NOT NULL,
	[WeekStart] [date] NOT NULL,
	[WeekEnd] [date] NOT NULL,
	[Targets] [varchar](max) NULL,
	[Progress] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[CampaignName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Configuration]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Configuration](
	[Name] [varchar](50) NOT NULL,
	[Value] [varchar](250) NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Facebook]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Facebook](
	[UserId] [uniqueidentifier] NOT NULL,
	[PageName] [varchar](255) NOT NULL,
	[PermanentAccessToken] [varchar](max) NULL,
	[FacebookData] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[PageName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LinkedIn]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LinkedIn](
	[UserId] [uniqueidentifier] NOT NULL,
	[AccessToken] [varchar](max) NOT NULL,
	[ExpirationDate] [datetime] NOT NULL,
	[LinkedInData] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MarketingModel]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MarketingModel](
	[Name] [varchar](50) NOT NULL,
	[Description] [varchar](max) NOT NULL,
	[MetricsUsed] [varchar](50) NOT NULL,
	[CalculationExpression] [varchar](max) NOT NULL,
	[ResultMetric] [varchar](50) NOT NULL,
 CONSTRAINT [PK_MarketingModel] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ScheduledPosts]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScheduledPosts](
	[PostId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[PostText] [varchar](255) NULL,
	[AttachmentPath] [varchar](255) NULL,
	[PostTime] [datetime] NOT NULL,
	[MediaChannels] [varchar](255) NOT NULL,
 CONSTRAINT [PK_ScheduledPosts] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Twitter]    Script Date: 26/08/2017 20:07:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Twitter](
	[UserId] [uniqueidentifier] NOT NULL,
	[AccessToken] [nvarchar](max) NOT NULL,
	[AccessTokenSecret] [nvarchar](max) NOT NULL,
	[ScreenName] [nvarchar](150) NOT NULL,
	[TwitterData] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ScreenName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[User]    Script Date: 26/08/2017 20:07:33 ******/
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
	[Campaigns] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
INSERT [dbo].[MarketingModel] ([Name], [Description], [MetricsUsed], [CalculationExpression], [ResultMetric]) VALUES (N'CPC', N'Used mainly for exposure and views of an update', N'Engagement', N'totalCost / engagement', N'Cost Per Click')
INSERT [dbo].[MarketingModel] ([Name], [Description], [MetricsUsed], [CalculationExpression], [ResultMetric]) VALUES (N'CPM', N'Used mainly for exposure and views of an update', N'Exposure', N'totalCost / exposure', N'Cost Per Thousand')
INSERT [dbo].[MarketingModel] ([Name], [Description], [MetricsUsed], [CalculationExpression], [ResultMetric]) VALUES (N'ROI', N'Used mainly for exposure and views of an update', N'All', N'price * ((engagement / influence) * (influence / engagement)) * engagement', N'Value returned from Social Media')
ALTER TABLE [dbo].[User] ADD  DEFAULT ((0)) FOR [Twitter]
GO
ALTER TABLE [dbo].[Facebook]  WITH CHECK ADD  CONSTRAINT [FK_Facebook_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Facebook] CHECK CONSTRAINT [FK_Facebook_User]
GO
ALTER TABLE [dbo].[LinkedIn]  WITH CHECK ADD  CONSTRAINT [FK_LinkedIn_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[LinkedIn] CHECK CONSTRAINT [FK_LinkedIn_User]
GO
ALTER TABLE [dbo].[Twitter]  WITH CHECK ADD  CONSTRAINT [FK_Twitter_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Twitter] CHECK CONSTRAINT [FK_Twitter_User]
GO
USE [master]
GO
ALTER DATABASE [FDM90] SET  READ_WRITE 
GO
