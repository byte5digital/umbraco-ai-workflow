// Thin client for GeoPreviewController using the backoffice bearer token.
const API_PATH = "/umbraco/management/api/v1/starterkit/geo";

export class GeoApiError extends Error {
    constructor(status) {
        super(`GEO API responded with ${status}`);
        this.status = status;
    }
}

export const createGeoApi = (authContext) => ({
    /** @returns {Promise<object>} GeoPagePreview of the saved (draft) document in the given culture */
    page: async (unique, culture, signal) => {
        const config = authContext.getOpenApiConfiguration();
        const query = culture ? `?${new URLSearchParams({ culture })}` : "";
        const response = await fetch(`${config.base}${API_PATH}/page/${encodeURIComponent(unique)}${query}`, {
            headers: { Authorization: `Bearer ${await config.token()}`, Accept: "application/json" },
            credentials: config.credentials,
            signal,
        });
        if (!response.ok) throw new GeoApiError(response.status);
        return response.json();
    },
});
