Vue.component('vue-file-item', {
    props: ['item'],
    template: `
        <li>
            <div class="file-item">
                <input class="file-item-block file-item-name" readonly="readonly"
                    v-bind:value="item.name" />
                <button class="file-item-block"
                    v-on:click="$emit('download', item)">
                    Download
                </button>
                <button class="file-item-block"
                    v-on:click="$emit('delete', item)">
                    Delete
                </button>
            </div>
        </li>`
});

Vue.component('vue-file-list', {
    props: ['items'],
    template: `
        <span class="file-list-empty"
            v-if="isEmpty()">
            no files
        </span>
        <ul class="file-list"
            v-else>
            <vue-file-item 
                v-for="item in items"
                v-bind:item="item"
                v-bind:key="item.jobId"
                v-on:download="$emit('download', item)"
                v-on:delete="$emit('delete', item)" />
        </ul>`,
    methods: {
        isEmpty() {
            return this.items?.length === 0;
        },
    }
});

var vm = new Vue({
    el: '#app',
    data: {
        items: []
    },
    methods: {
        async submitFiles() {
            var files = this.$refs.file.files;
            for (var i = 0; i < files.length; i++)
            {
                var formData = new FormData();
                formData.append("file", files[i]);
                
                this.addItem({
                    name: files[i].name,
                    jobId: await this.submitFile(formData)
                });
            }

            this.$refs.file.value = null;
        },
        async downloadItem(item) {
            let state = await this.getJobState(item.jobId);
            if (state == "Completed") {
                await this.downloadPdf(item.jobId);
                return;
            }
            alert(state);
        },
        addItem(item) {
            if (!this.isExists(item.jobId)) {
                this.items.push(item);
                this.saveItems();
            }
        },
        deleteItem(item) {
            var index = this.items.indexOf(item);
            if (index !== -1) {
                this.items.splice(index, 1);
                this.deleteJob(item.jobId);
            }
            this.saveItems();
        },
        clearItems() {
            while (this.items.length > 0)
            {
                this.deleteItem(this.items[0]);
            }
        },
        isExists(jobId) {
            return this.items.find(i => i.jobId === jobId) !== undefined;
        },
        saveItems() {
            sessionStorage.setItem("items", JSON.stringify(this.items));
        },
        loadItems() {
            return Array.prototype.slice.call(
                JSON.parse(sessionStorage.getItem("items")));
        },
        async submitFile(formData) {
            let jobId;
            await axios
                .post(serviceUrl + "/api/converter/jobs", formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                })
                .then(response => {
                    jobId = response.data.jobId;
                })
                .catch(error => {
                    console.log(error);
                    alert("Error");
                });
            return jobId;
        },
        async getJobState(jobId) {
            let state;
            await axios
                .get(serviceUrl + "/api/converter/details/" + jobId)
                .then(response => {
                    switch (response.status) {
                        case 200:
                            state = response.data.state;
                            break;
                        case 400:
                            state = "Not found";
                            break;
                    }
                })
                .catch(error => {
                    console.log(error);
                    alert("Error");
                });
            return state;
        },
        async downloadPdf(jobId) {
            await axios
                .get(serviceUrl + "/api/converter/results/" + jobId, { responseType: "blob" })
                .then(response => download(response.data, jobId + ".pdf"))
                .catch(error => {
                    console.log(error);
                    alert("Error");
                });
        },
        async deleteJob(jobId) {
            await axios
                .delete(serviceUrl + "/api/converter/jobs/" + jobId)
                .then(response => console.log("deleted job - " + jobId))
                .catch(error => console.log(error));
        }
    },
    created() {
        this.items = this.loadItems();
    }
});