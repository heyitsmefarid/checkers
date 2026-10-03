-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Apr 09, 2026 at 04:11 PM
-- Server version: 8.0.30
-- PHP Version: 8.1.10

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `checkers`
--

-- --------------------------------------------------------

--
-- Table structure for table `matchhistory`
--

CREATE TABLE `matchhistory` (
  `MatchID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `OpponentType` varchar(20) NOT NULL,
  `Difficulty` varchar(20) DEFAULT NULL,
  `Result` varchar(10) NOT NULL,
  `MovesCount` int DEFAULT NULL,
  `DatePlayed` datetime DEFAULT CURRENT_TIMESTAMP,
  `DurationSeconds` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `matchhistory`
--

INSERT INTO `matchhistory` (`MatchID`, `PlayerID`, `OpponentType`, `Difficulty`, `Result`, `MovesCount`, `DatePlayed`, `DurationSeconds`) VALUES
(2, 2, 'Computer', 'Hard', 'Loss', 64, '2026-03-31 01:20:34', NULL),
(3, 2, 'Computer', 'Hard', 'Loss', 28, '2026-04-01 03:15:53', NULL),
(4, 2, 'Player', NULL, 'Loss', 1, '2026-04-10 00:10:40', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `matchseries`
--

CREATE TABLE `matchseries` (
  `SeriesID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `BestOf` int NOT NULL,
  `Wins` int DEFAULT '0',
  `Losses` int DEFAULT '0',
  `FinalResult` varchar(10) DEFAULT NULL,
  `DatePlayed` datetime DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `matchseries`
--

INSERT INTO `matchseries` (`SeriesID`, `PlayerID`, `BestOf`, `Wins`, `Losses`, `FinalResult`, `DatePlayed`) VALUES
(2, 2, 1, 0, 1, 'Loss', '2026-04-01 03:15:53');

-- --------------------------------------------------------

--
-- Table structure for table `players`
--

CREATE TABLE `players` (
  `PlayerID` int NOT NULL,
  `Username` varchar(50) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `DateCreated` datetime DEFAULT CURRENT_TIMESTAMP,
  `LastLogin` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `players`
--

INSERT INTO `players` (`PlayerID`, `Username`, `FullName`, `Password`, `DateCreated`, `LastLogin`) VALUES
(2, 'farid', 'Fred Anthony', '12345', '2026-03-29 12:49:40', '2026-04-10 00:10:12');

-- --------------------------------------------------------

--
-- Table structure for table `stats`
--

CREATE TABLE `stats` (
  `StatsID` int NOT NULL,
  `PlayerID` int NOT NULL,
  `EasyWins` int DEFAULT '0',
  `NormalWins` int DEFAULT '0',
  `HardWins` int DEFAULT '0',
  `TotalWins` int DEFAULT '0',
  `TotalLosses` int DEFAULT '0',
  `BestWinSeconds` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `stats`
--

INSERT INTO `stats` (`StatsID`, `PlayerID`, `EasyWins`, `NormalWins`, `HardWins`, `TotalWins`, `TotalLosses`, `BestWinSeconds`) VALUES
(2, 2, 0, 0, 0, 0, 3, NULL);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `matchhistory`
--
ALTER TABLE `matchhistory`
  ADD PRIMARY KEY (`MatchID`),
  ADD KEY `PlayerID` (`PlayerID`);

--
-- Indexes for table `matchseries`
--
ALTER TABLE `matchseries`
  ADD PRIMARY KEY (`SeriesID`),
  ADD KEY `PlayerID` (`PlayerID`);

--
-- Indexes for table `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`PlayerID`),
  ADD UNIQUE KEY `Username` (`Username`);

--
-- Indexes for table `stats`
--
ALTER TABLE `stats`
  ADD PRIMARY KEY (`StatsID`),
  ADD KEY `PlayerID` (`PlayerID`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `matchhistory`
--
ALTER TABLE `matchhistory`
  MODIFY `MatchID` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `matchseries`
--
ALTER TABLE `matchseries`
  MODIFY `SeriesID` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `players`
--
ALTER TABLE `players`
  MODIFY `PlayerID` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `stats`
--
ALTER TABLE `stats`
  MODIFY `StatsID` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `matchhistory`
--
ALTER TABLE `matchhistory`
  ADD CONSTRAINT `matchhistory_ibfk_1` FOREIGN KEY (`PlayerID`) REFERENCES `players` (`PlayerID`);

--
-- Constraints for table `matchseries`
--
ALTER TABLE `matchseries`
  ADD CONSTRAINT `matchseries_ibfk_1` FOREIGN KEY (`PlayerID`) REFERENCES `players` (`PlayerID`);

--
-- Constraints for table `stats`
--
ALTER TABLE `stats`
  ADD CONSTRAINT `stats_ibfk_1` FOREIGN KEY (`PlayerID`) REFERENCES `players` (`PlayerID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
