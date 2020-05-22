﻿


/****** Script for SelectTopNRows command from SSMS  ******/ 
CREATE View [dbo].[EFTVendorsToEmail]
as
SELECT [f_no] as VCode, 
       [nom] as VendorName, 
       a.VendorID, 
       cl.contactid as ContactID, 
       co.firstname as FirstName, 
       co.lastname as LastName, 
       cg.contactgroupid as GroupID, 
       ce.email as Email 
FROM   [SPM_Database].[dbo].[EFTVendors] a 
       INNER JOIN [SPMDB].[dbo].[tccontactlinked] cl 
               ON cl.vendorid = a.VendorID 
       INNER JOIN [SPMDB].[dbo].[tccontact] co 
               ON co.id = cl.contactid 
       INNER JOIN [SPMDB].[dbo].[tccontactlinkedgroup] cg 
               ON cg.contactlinkedid = cl.id 
       INNER JOIN [SPMDB].[dbo].[tccontactemail] ce 
               ON ce.contactid = cl.contactid 
WHERE  a.actif <> 'N' 
       AND co.isactive = '1' 
       AND cg.contactgroupid = '1555' 
       AND ce.isprimary = '1' 
