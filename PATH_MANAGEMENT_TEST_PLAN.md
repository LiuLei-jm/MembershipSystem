# Path Management Solution - Test Plan

## Overview
This document outlines the test plan for verifying the path management solution that allows frontend control of file paths while maintaining consistency and security.

## Test Scenarios

### 1. Backend Path Configuration API
- [ ] Test GET /user/path-config endpoint returns correct default values
- [ ] Test POST /user/path-config endpoint updates path configuration
- [ ] Test validation of invalid paths (path traversal, invalid characters)
- [ ] Test validation of file names (invalid characters, length limits)

### 2. Path Service Functionality
- [ ] Test GetFullFilePath method with valid inputs
- [ ] Test GetFullFilePath method with invalid file names (should throw exceptions)
- [ ] Test IsPathValid method with various path formats
- [ ] Test path validation security measures (path traversal prevention)

### 3. File Operation Endpoints
- [ ] Test file append operations use dynamic paths
- [ ] Test file delete operations use dynamic paths
- [ ] Test security validation in file operations
- [ ] Test error handling for invalid paths

### 4. Frontend Integration
- [ ] Test PathConfiguration component loads current settings
- [ ] Test PathConfiguration component saves new settings
- [ ] Test CreateCardDialog uses dynamic base path
- [ ] Test frontend validation of path inputs

### 5. WPF Client Security
- [ ] Test IsValidFilePath method with valid paths
- [ ] Test IsValidFilePath method blocks path traversal attempts
- [ ] Test file operations in WPF client with valid paths
- [ ] Test file operations in WPF client reject invalid paths

### 6. Data Consistency
- [ ] Test that path configuration is persisted in database
- [ ] Test that path configuration is correctly loaded for users
- [ ] Test that expired membership processor uses correct paths
- [ ] Test that all file operations use consistent paths

### 7. Security Testing
- [ ] Test path traversal attacks are blocked
- [ ] Test invalid character injection is prevented
- [ ] Test maximum path length limits are enforced
- [ ] Test unauthorized access to path configuration is blocked

## Test Data
- Valid paths: "D:", "C:\\Users\\Test", "\\\\server\\share"
- Invalid paths: "..\\..\\secret.txt", "D:\\..\\windows", "/etc/passwd"
- Valid file names: "CDK.txt", "membership_cards.txt"
- Invalid file names: "../malicious.txt", "file\0name.txt", "a".repeat(300)

## Expected Results
- All valid operations should succeed with correct paths
- All invalid operations should be rejected with appropriate error messages
- Security measures should prevent path traversal and injection attacks
- Path configuration should be consistent across all components