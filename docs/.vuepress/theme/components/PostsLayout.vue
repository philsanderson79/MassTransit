<template>
    <div class="content__default">
        <h1>{{ $title }}</h1>

        <Pagination v-if="showPagination" />

        <ul class="blog-list">
            <li v-for="post in pages" class="blog-list__item">
                <PostPreview :item="post"/>
            </li>
        </ul>
    </div>
</template>

<script>
    import PostPreview from "@theme/components/PostPreview";
    import {Pagination, SimplePagination} from '@vuepress/plugin-blog/lib/client/components'

    export default {
        components: {PostPreview, Pagination, SimplePagination},

        computed: {
            pages() {
                return this.$pagination.pages
            },
            showPagination() {
                return this.$pagination._paginationPages.length > 1;
            }
        }
    }
</script>

<style scoped lang="stylus">
    .blog-list
        padding: 0
        margin: 0

    .blog-list__item
        list-style-type: none
</style>

