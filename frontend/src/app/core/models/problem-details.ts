/** RFC 9457 problem details returned by Portfolio.Api for every error response. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/**
 * Flatten FluentValidation 422 payloads (`errors: { Field: ["msg"] }`) into a
 * `{ Field: "msg" }` map for form field display. Returns an empty object for
 * responses without field errors.
 */
export function toFieldErrors(body: unknown): Record<string, string> {
  if (typeof body !== 'object' || body === null) {
    return {};
  }
  const errors = (body as ProblemDetails).errors;
  if (typeof errors !== 'object' || errors === null) {
    return {};
  }
  const fieldErrors: Record<string, string> = {};
  for (const [field, messages] of Object.entries(errors)) {
    const first = Array.isArray(messages) ? messages[0] : undefined;
    if (first) {
      fieldErrors[field] = first;
    }
  }
  return fieldErrors;
}
