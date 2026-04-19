export const PATIENT_REGISTRY_ROLES = ["Manager", "Admin", "Secretary", "Doctor", "Nurse"];
export const PATIENT_ENROLLMENT_ROLES = ["Manager", "Admin", "Secretary"];
export const APPOINTMENT_WORKSPACE_ROLES = ["Doctor", "Admin", "Manager", "Secretary"];
export const APPOINTMENT_DIRECTORY_ROLES = ["Admin", "Manager", "Secretary"];
export const APPOINTMENT_BOOKING_ROLES = ["Admin", "Secretary"];
export const APPOINTMENT_CANCELLATION_ROLES = ["Doctor", "Admin", "Manager", "Secretary"];
export const VISIT_WORKSPACE_ROLES = ["Doctor", "Nurse"];
export const TEST_RESULTS_WORKSPACE_ROLES = ["Doctor", "Nurse"];
export const SCHEDULE_WORKSPACE_ROLES = ["Manager", "Admin", "Doctor", "Nurse", "Secretary"];
export const STAFF_MANAGEMENT_ROLES = ["Manager", "Admin"];

export function hasRoleAccess(role, allowedRoles) {
  return allowedRoles.includes(role ?? "");
}
