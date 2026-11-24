// Shared plumbing of the GEO preview editor: live field values of the current document variant
// (property dataset), the saved-state preview from the server and a refresh after every save.
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_PROPERTY_DATASET_CONTEXT } from "@umbraco-cms/backoffice/property";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";
import { createGeoApi } from "./geo-api.js";

const ENTITY_UPDATED_EVENT = "entity-updated";

/**
 * @param {typeof import("@umbraco-cms/backoffice/external/lit").LitElement} Base
 * Subclasses declare `static fieldAliases = [...]` – their live values end up in `this._fields`.
 */
export const PreviewHostMixin = (Base) =>
    class extends UmbElementMixin(Base) {
        static properties = {
            value: { type: String },
            config: { attribute: false },
            _fields: { state: true },
            _name: { state: true },
            _preview: { state: true },
            _isLoading: { state: true },
            _error: { state: true },
        };

        #api;
        #unique;
        #culture;
        #actionEventContext;
        #requestController;
        #onEntityUpdated = (event) => {
            const unique = event.getUnique?.() ?? event.unique;
            if (unique === this.#unique) this.refresh();
        };

        constructor() {
            super();
            this._fields = Object.freeze({});
            this._name = "";
            this._preview = null;
            this._isLoading = false;
            this._error = null;

            this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
                if (!authContext) return;
                this.#api = createGeoApi(authContext);
                this.refresh();
            });

            this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (dataset) => {
                if (!dataset) return;
                this.#unique = dataset.getUnique();
                this.#culture = dataset.getVariantId?.()?.culture ?? undefined;
                this.observe(dataset.name, (name) => (this._name = name ?? ""), "_starterkitObserveName");
                this.constructor.fieldAliases.forEach((alias) => this.#observeField(dataset, alias));
                this.refresh();
            });

            this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
                this.#actionEventContext?.removeEventListener(ENTITY_UPDATED_EVENT, this.#onEntityUpdated);
                this.#actionEventContext = context;
                context?.addEventListener(ENTITY_UPDATED_EVENT, this.#onEntityUpdated);
            });
        }

        disconnectedCallback() {
            super.disconnectedCallback();
            this.#actionEventContext?.removeEventListener(ENTITY_UPDATED_EVENT, this.#onEntityUpdated);
            this.#requestController?.abort();
        }

        /** Reloads the server preview (based on the last saved version of the document). */
        async refresh() {
            if (!this.#api || !this.#unique) return;
            this.#requestController?.abort();
            this.#requestController = new AbortController();
            this._isLoading = true;
            try {
                this._preview = await this.#api.page(this.#unique, this.#culture, this.#requestController.signal);
                this._error = null;
            } catch (error) {
                if (error.name === "AbortError") return;
                // 404 = document not saved yet – the live parts of the preview keep working.
                this._preview = null;
                this._error = error.status === 404 ? null : "Vorschau konnte nicht geladen werden.";
            } finally {
                this._isLoading = false;
            }
        }

        async #observeField(dataset, alias) {
            const observable = await dataset.propertyValueByAlias(alias);
            if (!observable) return;
            this.observe(observable, (value) => {
                this._fields = Object.freeze({ ...this._fields, [alias]: value });
            }, `_starterkitObserve_${alias}`);
        }
    };

/** Trimmed string value of a field (labels/text boxes), empty string for anything else. */
export const textOf = (value) => (typeof value === "string" ? value.trim() : "");

/** True/false editor values arrive as booleans (older data: "1"/"0"). */
export const isOn = (value) => value === true || value === "1" || value === 1;
