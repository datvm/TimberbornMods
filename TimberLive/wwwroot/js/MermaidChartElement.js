const MermaidUrl = "https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs";

const service = new class {

    #init = false;
    #initPromise;
    #mermaid;

    async initAsync() {
        if (this.#init) {
            return this.#mermaid;
        }

        if (this.#initPromise) {
            return this.#initPromise;
        }

        this.#initPromise = (async () => {
            const module = await import(MermaidUrl);
            this.#mermaid = module.default;
            this.#mermaid.initialize({
                startOnLoad: false,
                flowchart: {
                    curve: "stepBefore"
                }
            });
            this.#init = true;
            return this.#mermaid;
        })();

        return this.#initPromise;
    }

}();

customElements.get("mermaid-chart") || customElements.define("mermaid-chart", class extends HTMLElement {

    static get observedAttributes() {
        return ["data-content"];
    }

    #container;
    #lastContent;
    #renderVersion = 0;

    connectedCallback() {
        if (!this.#container) {
            this.#container = document.createElement("div");
            this.append(this.#container);
            this.#container.addEventListener("click", (e) => this.#handleContainerClick(e));
        }

        this.#render();
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (name !== "data-content") {
            return;
        }

        if (oldValue === newValue) {
            return;
        }

        this.#render();
    }

    async #render() {
        const content = this.getAttribute("data-content") ?? "";
        if (content === this.#lastContent && this.#container?.childElementCount) {
            return;
        }

        const currentVersion = ++this.#renderVersion;
        const container = this.#container;
        if (!container) {
            return;
        }

        if (!content.trim()) {
            container.textContent = "";
            this.#lastContent = content;
            return;
        }

        try {
            const mermaid = await service.initAsync();
            if (currentVersion !== this.#renderVersion) {
                return;
            }

            const id = `mermaid-${crypto.randomUUID()}`;
            const { svg } = await mermaid.render(id, content);
            if (currentVersion !== this.#renderVersion) {
                return;
            }

            container.innerHTML = svg;
            this.#applyEdgeLabelStateClasses(container);
            this.#lastContent = content;
        } catch (error) {
            console.error("Failed to render mermaid chart", error);
            container.textContent = content;
            this.#lastContent = content;
        }
    }

    #handleContainerClick(e) {
        const svg = this.#container.querySelector("svg");
        if (!svg) {
            return;
        }

        const nodeGroup = this.#findNodeGroup(e.target);
        if (!nodeGroup) {
            return;
        }

        const entityId = this.#extractEntityIdFromNodeId(nodeGroup.id);
        if (entityId) {
            this.#dispatchNodeClickEvent(entityId);
        }
    }

    #findNodeGroup(element) {
        let current = element;
        while (current && current !== document) {
            if (current.tagName === "g" && current.id?.includes("Entity_") && this.#hasStateClass(current)) {
                return current;
            }
            current = current.parentElement;
        }
        return null;
    }

    #hasStateClass(element) {
        return element.classList.contains("state_on") || 
               element.classList.contains("state_off") || 
               element.classList.contains("state_error");
    }

    #extractEntityIdFromNodeId(nodeId) {
        const match = nodeId.match(/Entity_([a-f0-9]{32})/);
        return match ? match[1] : null;
    }

    #dispatchNodeClickEvent(entityId) {
        this.dispatchEvent(new CustomEvent("nodeclick", {
            detail: { entityId },
            bubbles: true,
            composed: true
        }));
    }

    #applyEdgeLabelStateClasses(container) {
        const svg = container.querySelector("svg");
        if (!svg) {
            return;
        }

        const labels = Array.from(svg.querySelectorAll("g.label[data-id^='Edge_']"));
        for (const label of labels) {
            const edgeDataId = label.getAttribute("data-id") ?? "";
            const entityId = this.#extractEntityId(edgeDataId);
            if (!entityId) {
                continue;
            }

            const node = svg.querySelector(`[id$='-${CSS.escape(entityId)}'], [id*='-${CSS.escape(entityId)}-']`);
            if (!node) {
                continue;
            }

            const stateClass = this.#getNodeStateClass(node);
            if (!stateClass) {
                continue;
            }

            const edgeStateClass = this.#toEdgeStateClass(stateClass);
            label.classList.remove("state_on", "state_off", "state_error", "edge_state_on", "edge_state_off", "edge_state_disconnected");
            label.classList.add(stateClass);
            if (edgeStateClass) {
                label.classList.add(edgeStateClass);
            }
        }

        const paths = Array.from(svg.querySelectorAll("path[data-id^='Edge_']"));
        for (const path of paths) {
            const edgeDataId = path.getAttribute("data-id") ?? "";
            const entityId = this.#extractEntityId(edgeDataId);
            if (!entityId) {
                continue;
            }

            const node = svg.querySelector(`[id$='-${CSS.escape(entityId)}'], [id*='-${CSS.escape(entityId)}-']`);
            if (!node) {
                continue;
            }

            const stateClass = this.#getNodeStateClass(node);
            if (!stateClass) {
                continue;
            }

            path.classList.remove("state_on", "state_off", "state_error", "edge_state_on", "edge_state_off", "edge_state_disconnected");
            path.classList.add(stateClass);
            if (stateClass) {
                path.classList.add(stateClass);
            }
        }
    }

    #extractEntityId(edgeDataId) {
        const parts = edgeDataId.split("_");
        const entityTokenIndex = parts.indexOf("Entity");
        if (entityTokenIndex < 0 || entityTokenIndex + 1 >= parts.length) {
            return null;
        }

        return `Entity_${parts[entityTokenIndex + 1]}`;
    }

    #toEdgeStateClass(stateClass) {
        if (stateClass === "state_on") {
            return "edge_state_on";
        }

        if (stateClass === "state_off") {
            return "edge_state_off";
        }

        if (stateClass === "state_error") {
            return "edge_state_disconnected";
        }

        return null;
    }

    #getNodeStateClass(node) {
        if (node.classList.contains("state_on")) {
            return "state_on";
        }

        if (node.classList.contains("state_off")) {
            return "state_off";
        }

        if (node.classList.contains("state_error")) {
            return "state_error";
        }

        return null;
    }

});