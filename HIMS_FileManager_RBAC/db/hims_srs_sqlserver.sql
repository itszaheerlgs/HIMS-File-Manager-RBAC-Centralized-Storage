-- --------------------------------------------------------
-- Host:                         172.10.102.124
-- Server version:               Microsoft SQL Server 2025 (RTM) - 17.0.1000.7
-- Server OS:                    Windows 10 Pro 10.0 <X64> (Build 26200: ) (Hypervisor)
-- HeidiSQL Version:             12.17.0.7270
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES  */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for hims_srs
CREATE DATABASE IF NOT EXISTS "hims_srs";
USE "hims_srs";

-- Dumping structure for table hims_srs.action_logs
CREATE TABLE IF NOT EXISTS "action_logs" (
	"id" INT,
	"user_name" NVARCHAR(255) DEFAULT N'N''⚠️ INTRUDER''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"role" NVARCHAR(50) DEFAULT N'N''HACKER_ATTEMPT''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"action_type" NVARCHAR(50) DEFAULT N'N''ILLEGAL_ACCESS''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"target_path" NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"ip_address" NVARCHAR(45) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"pc_name" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"user_agent" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"city" NVARCHAR(100) DEFAULT N'N''Unknown''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"region" NVARCHAR(100) DEFAULT N'N''Unknown''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"country" NVARCHAR(100) DEFAULT N'N''Unknown''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"latitude" NVARCHAR(50) DEFAULT N'N''0''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"longitude" NVARCHAR(50) DEFAULT N'N''0''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"isp" NVARCHAR(255) DEFAULT N'N''Unknown''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"log_time" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.admins
CREATE TABLE IF NOT EXISTS "admins" (
	"admin_id" INT,
	"username" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"password_hash" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"real_password" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"full_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"email" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"profile_pic_path" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"profile_pic_data" VARBINARY DEFAULT NULL,
	"profile_pic_mime" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"role" NVARCHAR(30) DEFAULT N'N''DataManager''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_active" BIT DEFAULT N'(1)',
	"last_activity" DATETIME2(7) DEFAULT NULL,
	"last_login" DATETIME2(7) DEFAULT NULL,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"is_superadmin" INT,
	"is_typing" BIT DEFAULT N'(0)',
	"last_public_message_id" INT DEFAULT N'(0)',
	"last_seen" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("admin_id"),
	UNIQUE INDEX "UQ_admins_email" ("email"),
	UNIQUE INDEX "UQ_admins_username" ("username"),
	CONSTRAINT "CK_admins_role" CHECK (([role]='StatisticianStaff' OR [role]='RecordControllScan' OR [role]='CertificationStaff' OR [role]='OPDStaff' OR [role]='Auditor' OR [role]='DataManager' OR [role]='SuperAdmin'))
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.attendance_records
CREATE TABLE IF NOT EXISTS "attendance_records" (
	"id" INT,
	"emp_id" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"log_date" DATE,
	"attendance_status" NVARCHAR(10) DEFAULT N'N''Present''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"late_minutes" INT DEFAULT N'(0)',
	"undertime_minutes" INT DEFAULT N'(0)',
	"total_deductions" INT DEFAULT N'(0)',
	"raw_logs" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"recorded_by" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	FOREIGN KEY INDEX "FK_attendance_records_employees" ("emp_id"),
	PRIMARY KEY ("id"),
	CONSTRAINT "FK_attendance_records_employees" FOREIGN KEY ("emp_id") REFERENCES "employees" ("emp_id") ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT "CK_attendance_status" CHECK (([attendance_status]='Absent' OR [attendance_status]='Half-Day' OR [attendance_status]='Present')),
	CONSTRAINT "CK_attendance_raw_logs_json" CHECK (([raw_logs] IS NULL OR isjson([raw_logs])=(1)))
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.case_entries
CREATE TABLE IF NOT EXISTS "case_entries" (
	"id" INT,
	"municipality_id" INT,
	"case_date_from" DATE,
	"medical_cases" INT DEFAULT N'(0)',
	"pediatric_cases" INT DEFAULT N'(0)',
	"surgical_adult_cases" INT DEFAULT N'(0)',
	"surgical_pedia_cases" INT DEFAULT N'(0)',
	"obstetrical_cases" INT DEFAULT N'(0)',
	"gyne_cases" INT DEFAULT N'(0)',
	"dental_cases" INT DEFAULT N'(0)',
	"total_cases" INT DEFAULT NULL,
	"case_date_to" DATE,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id"),
	UNIQUE INDEX "UQ_case_entries_municipality_date" ("case_date_to", "municipality_id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.case_name_list
CREATE TABLE IF NOT EXISTS "case_name_list" (
	"id" INT,
	"case_name" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.certificates
CREATE TABLE IF NOT EXISTS "certificates" (
	"id" INT,
	"certificate_type" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"original_filename" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"stored_path" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_by" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"upload_date" DATETIME2(7) DEFAULT N'sysdatetime()',
	"uploaded_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.certification_file_manager
CREATE TABLE IF NOT EXISTS "certification_file_manager" (
	"id" INT,
	"parent_id" INT DEFAULT N'(0)',
	"is_folder" BIT DEFAULT N'(0)',
	"display_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"system_path" NVARCHAR(500) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_type" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_size" BIGINT DEFAULT N'(0)',
	"file_data" VARBINARY DEFAULT NULL,
	"uploaded_by" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"file_path" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_locked" BIT DEFAULT N'(0)',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.chat_messages
CREATE TABLE IF NOT EXISTS "chat_messages" (
	"id" INT,
	"sender" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"role" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"message" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.datalogs
CREATE TABLE IF NOT EXISTS "datalogs" (
	"log_id" INT,
	"admin_id" INT,
	"username" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"full_name" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"action_type" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"log_timestamp" DATETIME2(7) DEFAULT N'sysdatetime()',
	"ip_address" NVARCHAR(45) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"user_agent" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("log_id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.employees
CREATE TABLE IF NOT EXISTS "employees" (
	"emp_id" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"full_name" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"department" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"status" NVARCHAR(10) DEFAULT N'N''Active''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("emp_id"),
	CONSTRAINT "CK_employees_status" CHECK (([status]='Inactive' OR [status]='Active'))
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.file_index
CREATE TABLE IF NOT EXISTS "file_index" (
	"id" BIGINT,
	"stored_path" NVARCHAR(600) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"filename" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_dir" BIT DEFAULT NULL,
	"size" BIGINT DEFAULT N'(0)',
	"uploaded_at" DATETIME2(7) DEFAULT NULL,
	"parent_path" NVARCHAR(600) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id"),
	UNIQUE INDEX "UQ_file_index_stored_path" ("stored_path")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.file_records
CREATE TABLE IF NOT EXISTS "file_records" (
	"id" INT,
	"parent_id" INT DEFAULT NULL,
	"name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_dir" BIT DEFAULT N'(0)',
	"size" BIGINT DEFAULT NULL,
	"mime_type" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"updated_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"full_path_hash" CHAR(32) COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id"),
	UNIQUE INDEX "UQ_file_records_full_path_hash" ("full_path_hash")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.formula_list
CREATE TABLE IF NOT EXISTS "formula_list" (
	"id" INT,
	"formula_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"formula_text" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_app_settings
CREATE TABLE IF NOT EXISTS "hims_app_settings" (
	"setting_key" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"setting_value" NVARCHAR(400) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"updated_by" NVARCHAR(150) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"updated_at" DATETIME2(7),
	PRIMARY KEY ("setting_key")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_audit_log
CREATE TABLE IF NOT EXISTS "hims_audit_log" (
	"id" INT,
	"performed_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"actor_id" INT,
	"actor_name" NVARCHAR(120) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"actor_role" NVARCHAR(60) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"module" NVARCHAR(60) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"action" NVARCHAR(80) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"target_id" NVARCHAR(60) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"target_name" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"detail" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"ip_address" NVARCHAR(45) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_chat_mentions
CREATE TABLE IF NOT EXISTS "hims_chat_mentions" (
	"id" INT,
	"message_id" INT,
	"mentioned_admin_id" INT,
	"mentioned_username" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"mentioned_by_id" INT,
	"mentioned_by_name" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"attached_item_id" INT DEFAULT NULL,
	"attached_item_name" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"attached_is_folder" BIT DEFAULT NULL,
	"created_at" DATETIME2(7),
	"is_read" BIT DEFAULT N'(0)',
	"read_at" DATETIME2(7) DEFAULT NULL,
	FOREIGN KEY INDEX "FK_hims_chat_mentions_message" ("message_id"),
	PRIMARY KEY ("id"),
	CONSTRAINT "FK_hims_chat_mentions_message" FOREIGN KEY ("message_id") REFERENCES "hims_chat_messages" ("id") ON UPDATE NO_ACTION ON DELETE NO_ACTION
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_chat_messages
CREATE TABLE IF NOT EXISTS "hims_chat_messages" (
	"id" INT,
	"sender_id" INT,
	"sender_name" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"recipient_id" INT DEFAULT NULL,
	"recipient_name" NVARCHAR(150) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"message" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"read_at" DATETIME2(7) DEFAULT NULL,
	"deleted_at" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_documents
CREATE TABLE IF NOT EXISTS "hims_documents" (
	"id" INT,
	"document_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"document_type" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"original_filename" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"stored_path" NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"mime_type" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_by" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_at" DATETIME2(7),
	"is_locked" BIT DEFAULT N'(0)',
	"locked_by" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"locked_at" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_suggestions
CREATE TABLE IF NOT EXISTS "hims_suggestions" (
	"id" INT,
	"user_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"suggestion" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7),
	"super_message" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"replied_at" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hims_unlock_requests
CREATE TABLE IF NOT EXISTS "hims_unlock_requests" (
	"id" INT,
	"file_id" INT,
	"requester_username" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"reason" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"status" NVARCHAR(10) DEFAULT N'N''pending''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"requested_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id"),
	CONSTRAINT "CK_hims_unlock_requests_status" CHECK (([status]='denied' OR [status]='approved' OR [status]='pending'))
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hospital_info
CREATE TABLE IF NOT EXISTS "hospital_info" (
	"id" INT,
	"hospital_name" NVARCHAR(150) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"hospital_code" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"authorized_beds" INT DEFAULT N'(0)',
	"municipality_id" INT DEFAULT NULL,
	"address" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"contact_number" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"email" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"chief_hospital" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"license_number" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"ownership_type" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"hospital_level" NVARCHAR(20) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.hospital_statistics
CREATE TABLE IF NOT EXISTS "hospital_statistics" (
	"id" INT,
	"from_date" DATE,
	"to_date" DATE,
	"inpatient_service_days" INT DEFAULT N'(0)',
	"authorized_beds" INT DEFAULT N'(0)',
	"discharges_alive" INT DEFAULT N'(0)',
	"deaths_total" INT DEFAULT N'(0)',
	"deaths_over_48hrs" INT DEFAULT N'(0)',
	"infections_total" INT DEFAULT N'(0)',
	"vap" INT DEFAULT N'(0)',
	"bsi" INT DEFAULT N'(0)',
	"uti" INT DEFAULT N'(0)',
	"ssi" INT DEFAULT N'(0)',
	"opd_visits" INT DEFAULT N'(0)',
	"er_visits" INT DEFAULT N'(0)',
	"cs_deliveries" INT DEFAULT N'(0)',
	"total_deliveries" INT DEFAULT N'(0)',
	"maternal_deaths" INT DEFAULT N'(0)',
	"perinatal_deaths" INT DEFAULT N'(0)',
	"fetal_deaths" INT DEFAULT N'(0)',
	"total_admissions" INT DEFAULT N'(0)',
	"total_discharges" INT DEFAULT N'(0)',
	"total_days" INT DEFAULT N'(0)',
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.logs
CREATE TABLE IF NOT EXISTS "logs" (
	"id" INT,
	"action" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"details" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7),
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.messages
CREATE TABLE IF NOT EXISTS "messages" (
	"id" INT,
	"sender_username" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"recipient_username" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"message" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.municipal_case_reports
CREATE TABLE IF NOT EXISTS "municipal_case_reports" (
	"entry_id" INT,
	"admin_id" INT,
	"municipality_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"case_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"case_date" DATE,
	"case_count" INT,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("entry_id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.municipality_list
CREATE TABLE IF NOT EXISTS "municipality_list" (
	"id" INT,
	"municipality_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.opd_file_manager
CREATE TABLE IF NOT EXISTS "opd_file_manager" (
	"id" INT,
	"parent_id" INT DEFAULT N'(0)',
	"is_folder" BIT DEFAULT N'(0)',
	"display_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"system_path" NVARCHAR(500) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_type" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_size" BIGINT DEFAULT N'(0)',
	"file_data" VARBINARY DEFAULT NULL,
	"uploaded_by" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"file_path" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_locked" BIT DEFAULT N'(0)',
	"is_deleted" BIT DEFAULT N'(0)',
	"deleted_by" NVARCHAR(150) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"deleted_at" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.password_requests
CREATE TABLE IF NOT EXISTS "password_requests" (
	"id" INT,
	"fullname" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"username" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"gmail" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"status" NVARCHAR(10) DEFAULT N'N''Pending''' COLLATE SQL_Latin1_General_CP1_CI_AS,
	"token" NVARCHAR(255) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id"),
	CONSTRAINT "CK_password_requests_status" CHECK (([status]='Decline' OR [status]='Resolved' OR [status]='Pending'))
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.password_resets
CREATE TABLE IF NOT EXISTS "password_resets" (
	"id" INT,
	"email" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"token" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"expires_at" DATETIME2(7),
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.patient_documents
CREATE TABLE IF NOT EXISTS "patient_documents" (
	"id" INT,
	"patient_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"folder_name_safe" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"HRN" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_name_original" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_name_system" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"file_path" NVARCHAR(512) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"mime_type" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_by" NVARCHAR(50) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"uploaded_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"opd_date" DATE,
	"lastname" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"firstname" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"middlename" NVARCHAR(100) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"birthdate" DATE DEFAULT NULL,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.records
CREATE TABLE IF NOT EXISTS "records" (
	"id" INT,
	"path" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_folder" BIT DEFAULT N'(0)',
	"size" BIGINT DEFAULT N'(0)',
	"hash" CHAR(32) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7),
	PRIMARY KEY ("id"),
	UNIQUE INDEX "UQ_records_path" ("path")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.share_links
CREATE TABLE IF NOT EXISTS "share_links" (
	"id" INT,
	"file_id" INT,
	"token" CHAR(16) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"expire_at" DATETIME2(7) DEFAULT NULL,
	FOREIGN KEY INDEX "FK_share_links_file_records" ("file_id"),
	PRIMARY KEY ("id"),
	UNIQUE INDEX "UQ_share_links_token" ("token"),
	CONSTRAINT "FK_share_links_file_records" FOREIGN KEY ("file_id") REFERENCES "file_records" ("id") ON UPDATE NO_ACTION ON DELETE CASCADE
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.shares
CREATE TABLE IF NOT EXISTS "shares" (
	"token" CHAR(16) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"path" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"is_folder" BIT,
	"created_at" DATETIME2(7),
	"expire_at" DATETIME2(7) DEFAULT NULL,
	PRIMARY KEY ("token")
);

-- Data exporting was unselected.

-- Dumping structure for procedure hims_srs.sp_UpdateLastLogin
DELIMITER //
CREATE PROCEDURE dbo.sp_UpdateLastLogin
    @AdminId INT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.admins SET last_login = SYSDATETIME() WHERE admin_id = @AdminId;
END
//
DELIMITER ;

-- Dumping structure for procedure hims_srs.sp_ValidateLogin
DELIMITER //
CREATE PROCEDURE dbo.sp_ValidateLogin
    @Username NVARCHAR(50)
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT admin_id, username, full_name, role,
           password_hash, real_password, profile_pic_path
    FROM   dbo.admins
    WHERE  username = @Username AND is_active = 1;
END
//
DELIMITER ;

-- Dumping structure for table hims_srs.statistical_results
CREATE TABLE IF NOT EXISTS "statistical_results" (
	"id" INT,
	"formula_id" NVARCHAR(10) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"formula_name" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"result_value" DECIMAL(10,2),
	"period_start" DATE,
	"period_end" DATE,
	"input_parameters" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	"created_at" DATETIME2(7) DEFAULT N'sysdatetime()',
	"parameter_details" NVARCHAR(max) DEFAULT NULL COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.system_logs
CREATE TABLE IF NOT EXISTS "system_logs" (
	"id" INT,
	"timestamp" DATETIME2(7),
	"user_full_name" NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"action_type" NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"description" NVARCHAR(max) COLLATE SQL_Latin1_General_CP1_CI_AS,
	PRIMARY KEY ("id")
);

-- Data exporting was unselected.

-- Dumping structure for table hims_srs.user_online
CREATE TABLE IF NOT EXISTS "user_online" (
	"username" NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS,
	"last_seen" DATETIME2(7) DEFAULT N'sysdatetime()',
	PRIMARY KEY ("username")
);

-- Data exporting was unselected.

-- Dumping structure for trigger hims_srs.trg_file_records_updated_at
/* SQL Error (156): Incorrect syntax near the keyword 'FROM'. */-- Dumping structure for trigger hims_srs.trg_user_online_last_seen
/* SQL Error (156): Incorrect syntax near the keyword 'FROM'. *//*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
