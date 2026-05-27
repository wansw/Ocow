<template>
  <main class="page">
    <section class="content-panel">
      <el-result title="页面不存在" sub-title="当前路由没有对应后台页面">
        <template #extra>
          <el-button type="primary" @click="$router.replace('/')">返回工作台</el-button>
        </template>
      </el-result>
    </section>
  </main>
</template>
