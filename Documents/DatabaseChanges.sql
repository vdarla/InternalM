-- Version 1.0.1.2
ALTER TABLE t_transaction
ADD ACKStatus INT;
-------------

USE [MedicalDev]
GO
/****** Object:  StoredProcedure [dbo].[GetLicenses]    Script Date: 23-Jun-17 12:10:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Raj K>
-- Create date: <22-05-2017>
-- Description:	<Gets Licenses>
-- =============================================
ALTER PROCEDURE [dbo].[GetLicenses]
	-- Add the parameters for the stored procedure here
	@UserId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT T.Id, T.ServiceId, T.LicenseNumber, FORMAT(T.LicenseIssuedDate, 'dd-MM-yyyy') AS LicenseIssuedDate, 
	FORMAT(T.LicenseExpiryDate, 'dd-MM-yyyy') AS LicenseExpiryDate, D.[Name] AS District,
	M.[Name] AS Mandal, V.[Name] AS Village, T.ACKStatus
	FROM t_transaction T JOIN  m_district D ON T.DistrictId = D.Id
	JOIN m_mandal M ON T.MandalId = M.Id
	JOIN m_village V ON T.VillageId = V.Id
	WHERE T.StatusId = 7 AND T.CreatedUserId = @UserId;
END
---------------