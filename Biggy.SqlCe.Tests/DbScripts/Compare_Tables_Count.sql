/*
	Script to compare records counts in tables in Sql server and SqlCe
	to test SqlCe database creation script
 */

USE [Chinook];

-- User Table information:
-- Number of tables: 11
-- Album: 347 row(s)
-- Artist: 275 row(s)
-- Customer: 59 row(s)
-- Employee: 8 row(s)
-- Genre: 25 row(s)
-- Invoice: 412 row(s)
-- InvoiceLine: 2240 row(s)
-- MediaType: 5 row(s)
-- Playlist: 18 row(s)
-- PlaylistTrack: 8715 row(s)
-- Track: 3503 row(s)

SELECT Count(1) AS 'Album' FROM Album
SELECT Count(1) AS 'Artist' FROM Artist
SELECT Count(1) AS 'Customer' FROM Customer
SELECT Count(1) AS 'Employee' FROM Employee
SELECT Count(1) AS 'Genre' FROM Genre
SELECT Count(1) AS 'Invoice' FROM Invoice
SELECT Count(1) AS 'InvoiceLine' FROM InvoiceLine
SELECT Count(1) AS 'MediaType' FROM MediaType
SELECT Count(1) AS 'Playlist' FROM Playlist
SELECT Count(1) AS 'PlaylistTrack' FROM PlaylistTrack
SELECT Count(1) AS 'Track' FROM Track