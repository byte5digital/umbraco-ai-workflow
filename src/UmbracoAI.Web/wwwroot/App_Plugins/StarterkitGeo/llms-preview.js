// Property editor UI "llms.txt & Markdown": how AI agents see this page – live llms.txt entry plus the
// Markdown twin of the page (saved state, refreshed after every save).
import { LitElement, html, css, nothing } from "@umbraco-cms/backoffice/external/lit";
import { PreviewHostMixin, isOn, textOf } from "./preview-host.mixin.js";
import { previewStyles } from "./preview.styles.js";

const COPY_FEEDBACK_MS = 1500;

const EXCLUSION_LABELS = Object.freeze({
    noindex: "Ausgeschlossen: noindex (SEO-Tab)",
    excluded: "Ausgeschlossen: „Aus llms.txt ausschließen“",
});

export default class StarterkitLlmsPreview extends PreviewHostMixin(LitElement) {
    static fieldAliases = ["llmSummary", "metaDescription", "llmsExclude", "llmsOptional", "robotsNoIndex", "llmKeyFacts"];

    static styles = [previewStyles, css`
        h4 {
            margin: var(--uui-size-space-5) 0 var(--uui-size-space-2);
            font-size: 13px;
        }

        pre {
            margin: 0;
            padding: var(--uui-size-space-4);
            border-radius: var(--uui-border-radius);
            background: #0f1b2d;
            color: #e6edf3;
            font: 12px/1.6 ui-monospace, SFMono-Regular, Menlo, monospace;
            white-space: pre-wrap;
            word-break: break-word;
            overflow: auto;
        }

        pre.markdown {
            max-height: 420px;
        }

        .links {
            display: flex;
            flex-wrap: wrap;
            gap: var(--uui-size-space-2);
        }
    `];

    _copied = false;

    #liveExclusion() {
        if (isOn(this._fields.robotsNoIndex)) return "noindex";
        if (isOn(this._fields.llmsExclude)) return "excluded";
        return null;
    }

    /** The llms.txt line with the current (possibly unsaved) values. */
    #liveEntry() {
        const url = this._preview?.markdownUrl || "(URL nach Veröffentlichung)";
        const summary = textOf(this._fields.llmSummary) || textOf(this._fields.metaDescription) || textOf(this._preview?.fallbackDescription ?? "");
        const entry = `- [${this._name}](${url})`;
        return summary ? `${entry}: ${summary}` : entry;
    }

    #status(isHome) {
        const exclusion = this.#liveExclusion();
        if (exclusion) return { label: EXCLUSION_LABELS[exclusion], rating: "danger" };
        if (this._preview && !this._preview.isPublished) return { label: "Nicht veröffentlicht – noch nicht sichtbar", rating: "warning" };
        if (isHome) return { label: "Startseite = Kopfbereich von llms.txt", rating: "good" };
        if (isOn(this._fields.llmsOptional)) return { label: "In llms.txt (Abschnitt „Optional“)", rating: "good" };
        return { label: "In llms.txt enthalten", rating: "good" };
    }

    async #copyMarkdown() {
        if (!this._preview?.markdown) return;
        await navigator.clipboard.writeText(this._preview.markdown);
        this._copied = true;
        this.requestUpdate();
        setTimeout(() => {
            this._copied = false;
            this.requestUpdate();
        }, COPY_FEEDBACK_MS);
    }

    #renderLinks() {
        const preview = this._preview;
        if (!preview) return nothing;
        const link = (href, label, icon) => href
            ? html`<uui-button look="outline" compact href=${href} target="_blank" label=${label}><uui-icon name=${icon}></uui-icon> ${label}</uui-button>`
            : nothing;
        return html`<div class="links">
            ${link(preview.markdownUrl, "Markdown-Version", "icon-document")}
            ${link(preview.llmsUrl, "llms.txt", "icon-list")}
            ${link(preview.llmsFullUrl, "llms-full.txt", "icon-book-alt")}
        </div>`;
    }

    #renderEntry(isHome) {
        if (isHome) {
            return html`<p class="hint">Die Startseite liefert Titel, Einleitung (llms.txt-Einleitung bzw. KI-Zusammenfassung) und Kernfakten im Kopf von llms.txt.</p>`;
        }
        const liveEntry = this.#liveEntry();
        const isDirty = this._preview?.llmsEntry && this._preview.llmsEntry !== liveEntry;
        return html`
            <h4>Eintrag in llms.txt (live)</h4>
            <pre>${liveEntry}</pre>
            ${isDirty ? html`<p class="hint">Ungespeicherte Änderungen – nach dem Speichern wird auch die Markdown-Version aktualisiert.</p>` : nothing}
        `;
    }

    render() {
        const preview = this._preview;
        const isHome = Boolean(preview) && !preview.llmsEntry;
        const status = this.#status(isHome);
        return html`
            <div class="toolbar">
                <span class="check check--${status.rating}">${status.label}</span>
                ${preview ? html`<span class="check">≈ ${preview.estimatedTokens.toLocaleString("de-DE")} Tokens</span>` : nothing}
                <span class="spacer"></span>
                ${this._isLoading ? html`<uui-loader-circle></uui-loader-circle>` : nothing}
                <uui-button look="secondary" compact label="Aktualisieren" @click=${() => this.refresh()}>
                    <uui-icon name="icon-refresh"></uui-icon> Aktualisieren
                </uui-button>
            </div>
            ${this.#renderLinks()}
            ${this.#renderEntry(isHome)}
            ${preview ? html`
                <h4>Markdown-Version für Agenten (gespeicherter Stand)</h4>
                <pre class="markdown">${preview.markdown}</pre>
                <div class="toolbar" style="margin-top: var(--uui-size-space-3)">
                    <uui-button look="secondary" compact label="Markdown kopieren" @click=${() => this.#copyMarkdown()}>
                        <uui-icon name=${this._copied ? "icon-check" : "icon-documents"}></uui-icon> ${this._copied ? "Kopiert" : "Markdown kopieren"}
                    </uui-button>
                </div>` : html`<p class="hint">Speichern Sie die Seite, um die Markdown-Vorschau zu sehen.</p>`}
            ${this._error ? html`<p class="hint">${this._error}</p>` : nothing}
        `;
    }
}

customElements.define("starterkit-llms-preview", StarterkitLlmsPreview);
