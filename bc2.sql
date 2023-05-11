/*
 Navicat Premium Data Transfer

 Source Server         : A
 Source Server Type    : MySQL
 Source Server Version : 50730 (5.7.30)
 Source Host           : localhost:3307
 Source Schema         : bc2

 Target Server Type    : MySQL
 Target Server Version : 50730 (5.7.30)
 File Encoding         : 65001
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for dogtags
-- ----------------------------
DROP TABLE IF EXISTS `dogtags`;
CREATE TABLE `dogtags`  (
  `pid` int(11) NOT NULL,
  `key` int(11) NOT NULL,
  `val` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`pid`, `key`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of dogtags
-- ----------------------------

-- ----------------------------
-- Table structure for ranks
-- ----------------------------
DROP TABLE IF EXISTS `ranks`;
CREATE TABLE `ranks`  (
  `pid` int(11) NOT NULL,
  `rank` json NULL,
  PRIMARY KEY (`pid`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of ranks
-- ----------------------------

-- ----------------------------
-- Table structure for servers
-- ----------------------------
DROP TABLE IF EXISTS `servers`;
CREATE TABLE `servers`  (
  `gid` int(11) NOT NULL,
  `gdat` json NULL,
  `gdet` json NULL,
  PRIMARY KEY (`gid`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of servers
-- ----------------------------

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users`  (
  `UId` int(11) NOT NULL AUTO_INCREMENT,
  `UName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `UKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `PId` int(11) NULL DEFAULT 0,
  `PName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `PKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `InTime` datetime NULL DEFAULT NULL,
  `OutTime` datetime NULL DEFAULT NULL,
  `RegTime` datetime NULL DEFAULT NULL,
  `Ip` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Mac` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Ban` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT '0',
  `BanTime` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`UId`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 100000 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of users
-- ----------------------------

SET FOREIGN_KEY_CHECKS = 1;
