﻿<style lang="less">
.Kyq{
    .vertical-center-modal{
        display: flex;
        align-items: center;
        justify-content: center;
        .ivu-modal{
            top: 0;
        }
    }
    .update-paste{
      &-con{
        height: 350px;
      }
      &-btn-con{
        box-sizing: content-box;
        height: 30px;
        padding: 15px 0 5px;
      }
    }
    .paste-tip{
      color: #19be6b;
    }
    .ivu-modal-footer {
      padding: 1px 18px 8px 18px !important;
    }
    .CodeMirror-sizer{
      margin-left: 30px !important;
    }
    .CodeMirror-linenumbers{
      width: 29px !important;
    }
    .tdtd{
      color: #fff;
      padding: 10px 0;
      text-align: center;
      background: rgba(0,153,229,.5);
      width: 25%;
      float: left;
    }
    .tdtdselect{
      background: rgba(0,153,229,1) !important;
    }
  }  
</style>
<template>
  <div class="Kyq">
    <Modal  class="Kyq"
        v-model="modal1"
        width=800
        title="批量录入">
        <Row  ref="m1n1">
            <i-col  class="tdtd">空预器名称</i-col><i-col  class="tdtd">开口方向</i-col><i-col  class="tdtd">转动方向</i-col><i-col  class="tdtd">锅炉描述</i-col>
        </Row>
        <Row :gutter="10">
          <i-col span="24">
            <Card>
              <div class="update-paste-con">
                <paste-editor v-model="pasteDataArr" @on-success="handleSuccess" @on-error="handleError" @input="handleInput"  :colnumref="4" />
              </div>
              <div class="update-paste-btn-con">
                <span class="paste-tip">使用Tab键换列，使用回车键换行</span>
                <Button type="primary" style="float: right;" @click="handleShow">数据上传</Button>
              </div>
            </Card>
          </i-col>
        </Row>
        <div slot="footer">
        </div>
    </Modal>
    <Card>
      <tables
        ref="tables"
        editable
        searchable
        :border="false"
        size="small"
        search-place="top"
        v-model="stores.Kyq.data"
        :totalCount="stores.Kyq.query.totalCount"
        :pageSize="stores.Kyq.query.pageSize"
        :columns="stores.Kyq.columns"
        @on-delete="handleDelete"
        @on-edit="handleEdit"
        @on-select="handleSelect"
        @on-selection-change="handleSelectionChange"
        @on-refresh="handleRefresh"
        :row-class-name="rowClsRender"
        @on-page-change="handlePageChanged"
        @on-page-size-change="handlePageSizeChanged"
        @on-sort-change="handleSortChange"
      >
        <div slot="search">
          <section class="dnc-toolbar-wrap">
            <Row :gutter="16">
              <Col span="16">
                <Form inline @submit.native.prevent>
                  <FormItem>
                    <Input
                      type="text"
                      search
                      :clearable="true"
                      v-model="stores.Kyq.query.kw"
                      placeholder="输入空预器名称/搜索..."
                      @on-search="handleSearchKyq()"
                    >
                      <Select
                        slot="prepend"
                        v-model="stores.Kyq.query.isDeleted"
                        @on-change="handleSearchKyq"
                        placeholder="删除状态"
                        style="width:60px;"
                      >
                        <Option
                          v-for="item in stores.Kyq.sources.isDeletedSources"
                          :value="item.value"
                          :key="item.value"
                        >{{item.text}}</Option>
                      </Select>
                      <Select
                          slot="prepend"
                          v-model="stores.Kyq.query.boilerid"
                          @on-change="handleSearchKyq"
                          placeholder="机组"
                          style="width:100px;"
                        >
                        <Option
                            value="-1"
                            key="-1"
                          >不限机组</Option>
                          <Option
                            v-for="item in boilers"
                            :value="item.value"
                            :key="item.value"
                          >{{item.text}}</Option>
                      </Select>
                      <!-- <Select
                        slot="prepend"
                        v-model="stores.Kyq.query.status"
                        @on-change="handleSearchKyq"
                        placeholder="状态"
                        style="width:60px;"
                      >
                        <Option
                          v-for="item in stores.Kyq.sources.statusSources"
                          :value="item.value"
                          :key="item.value"
                        >{{item.text}}</Option>
                      </Select> -->
                    </Input>
                  </FormItem>
                </Form>
              </Col>
              <Col span="8" class="dnc-toolbar-btns">
                <ButtonGroup class="mr3">
                  <Button
                    class="txt-danger"
                    icon="md-trash"
                    title="删除"
                    @click="handleBatchCommand('delete')"
                  ></Button>
                  <Button
                    class="txt-success"
                    icon="md-redo"
                    title="恢复"
                    @click="handleBatchCommand('recover')"
                  ></Button>
                  <!-- <Button
                    class="txt-danger"
                    icon="md-hand"
                    title="禁用"
                    @click="handleBatchCommand('forbidden')"
                  ></Button>
                  <Button
                    class="txt-success"
                    icon="md-checkmark"
                    title="启用"
                    @click="handleBatchCommand('normal')"
                  ></Button> -->
                  <Button icon="md-refresh" title="刷新" @click="handleRefresh"></Button>
                  <!-- <Button icon="md-list-box" title="批量录入" @click="handleInputData" ></Button> -->
                </ButtonGroup>
                <Button
                  icon="md-create"
                  type="primary"
                  @click="handleShowCreateWindow"
                  title="新增"
                >新增</Button>
              </Col>
            </Row>
          </section>
        </div>
      </tables>
    </Card>
    <Drawer
      :title="formTitle"
      v-model="formModel.opened"
      width="600"
      :mask-closable="false"
      :mask="true"
      :styles="styles"
    >
      <Form :model="formModel.fields" ref="formKyq" :rules="formModel.rules" label-position="top">
        <Row :gutter="16">
                    <Col span="12">
                    <FormItem label="空预器名称" prop="K_Name_kw" >
                      <Input v-model="formModel.fields.K_Name_kw" placeholder="请输入空预器名称"/>
                    </FormItem>
                    </Col>
                
                    <Col span="12">
                    <FormItem label="开口方向" prop="LR">
                      <i-switch size="large" v-model="formModel.fields.Lr" :true-value="1" :false-value="0">
                        <span slot="open">右</span>
                        <span slot="close">左</span>
                      </i-switch>
                    </FormItem>
                    </Col>
                </Row><Row :gutter="16">
                    <Col span="12">
                    <FormItem label="转动方向" prop="Way">
                      <i-switch size="large" v-model="formModel.fields.Way" :true-value="1" :false-value="0">
                        <span slot="open">逆时针</span>
                        <span slot="close">顺时针</span>
                      </i-switch>
                    </FormItem>
                    </Col>
                
                    <Col span="12">
                    <FormItem label="锅炉ID"  prop="DncBoiler_Name">
                          <Select v-model="formModel.fields.DncBoiler_Name" placeholder="锅炉ID">
                            <Option
                              v-for="item in formSource.DncBoiler "
                              :value="item.text"
                              :disabled="item.disabled"
                              :key="item.text"
                            >{{item.text}}</Option>
                          </Select>
                    </FormItem>
                    </Col>
                </Row></Row>
        <Row :gutter="16">
        <!-- 
        <Col span="12">
        <FormItem label="状态" >
          <i-switch size="large" v-model="formModel.fields.Status" :true-value="1" :false-value="0">
            <span slot="open">正常</span>
            <span slot="close">禁用</span>
          </i-switch>
        </FormItem>
        </Col>
        -->
        </Row>
        

      </Form>
      <div class="demo-drawer-footer">
        <Button icon="md-checkmark-circle" type="primary" @click="handleSubmitKyq">保 存</Button>
        <Button style="margin-left: 8px" icon="md-close" @click="formModel.opened = false">取 消</Button>
      </div>
    </Drawer>
  </div>
</template>

<script>
import PasteEditor from '_c/paste-editor'
import { getTableDataFromArray } from '@/libs/util'
import Tables from "_c/tables";
import {
  getDateMore,
  upperKey
} from "@/libs/tools";
import { getBoilerList,getBoilerListAll } from "@/api/ZNRS/Dncboiler";
import {
  getKyqList,
  createKyq,
  loadKyq,
  editKyq,
  deleteKyq,
  batchCommand,
  batchCreateKyq
} from "@/api/ZNRS/Dnckyq";
export default {
  name: "ZNRS_Kyq_page",
  components: {
    Tables,PasteEditor
  },
  data() {
    return {
      boilers:[],
      dataorder:["K_Name_kw","LR","Way","DncBoiler_Name"],
      pasteDataArr: [],
      columns: [],
      validated: true,
      errorIndex: 0,
      modal1:false,
      commands: {
        delete: { name: "delete", title: "删除" },
        recover: { name: "recover", title: "恢复" },
        forbidden: { name: "forbidden", title: "禁用" },
        normal: { name: "normal", title: "启用" }
      },
      formModel: {
        opened: false,
        title: "创建",
        mode: "create",
        selection: [],
        fields: {

        },
        rules: {
          K_Name_kw: [
            {
              type: "string",
              required: true,
              message: "请输入空预器名称",
              min: 1
            }
          ],
           DncBoiler_Name: [
            {
              type: "string",
              required: true,
              message: "请输入锅炉ID",
              min: 1
            }
          ], 
        }
      },
      stores: {
        Kyq: {
          query: {
            totalCount: 0,
            pageSize: 20,
            currentPage: 1,
            kw: "",
            isDeleted: 0,
            status: -1,
            boilerid: -1,
            sort: [
              {
                direct: "DESC",
                field: "id"
              }
            ]
          },
          sources: {
            isDeletedSources: [
              { value: -1, text: "全部" },
              { value: 0, text: "正常" },
              { value: 1, text: "已删" }
            ],
            statusSources: [
              { value: -1, text: "全部" },
              { value: 0, text: "禁用" },
              { value: 1, text: "正常" }
            ],
            statusFormSources: [
              { value: 0, text: "禁用" },
              { value: 1, text: "正常" }
            ]
          },
          columns: [
            { type: "selection", width: 50, key: "handle" },
            { title: "空预器名称", key: "k_Name_kw",  sortable: "custom" },    
            { title: "开口方向", key: "lr",  sortable: "custom" ,
              render: (h, params) => {
                let realTime = params.row.lr;
                if(realTime==0){
                  realTime= "左";
                }else{
                  realTime = "右";
                }
                return h('span', [
                                h('strong', realTime)
                            ]);
              }
            },    
            { title: "转动方向", key: "way",  sortable: "custom" ,
              render: (h, params) => {
                let realTime = params.row.way;
                if(realTime==0){
                  realTime= "顺时针";
                }else{
                  realTime = "逆时针";
                }
                return h('span', [
                                h('strong', realTime)
                            ]);
              }},    
            { title: "锅炉描述", key: "dncBoiler_Name",  sortable: "custom" },    
            {
              title: "状态",
              key: "status",
              align: "center",
              width: 120,
              render: (h, params) => {
                let status = params.row.status;
                let statusColor = "success";
                let statusText = "正常";
                switch (status) {
                  case 0:
                    statusText = "禁用";
                    statusColor = "default";
                    break;
                }
                return h(
                  "Tooltip",
                  {
                    props: {
                      placement: "top",
                      transfer: true,
                      delay: 500
                    }
                  },
                  [
                    //这个中括号表示是Tooltip标签的子标签
                    h(
                      "Tag",
                      {
                        props: {
                          //type: "dot",
                          color: statusColor
                        }
                      },
                      statusText
                    ), //表格列显示文字
                    h(
                      "p",
                      {
                        slot: "content",
                        style: {
                          whiteSpace: "normal"
                        }
                      },
                      statusText //整个的信息即气泡内文字
                    )
                  ]
                );
              }
            },
            {
              title: "操作",
              align: "center",
              key: "handle",
              width: 150,
              className: "table-command-column",
              options: ["edit"],
              button: [
                (h, params, vm) => {
                  return h(
                    "Poptip",
                    {
                      props: {
                        confirm: true,
                        title: "你确定要删除吗?"
                      },
                      on: {
                        "on-ok": () => {
                          vm.$emit("on-delete", params);
                        }
                      }
                    },
                    [
                      h(
                        "Tooltip",
                        {
                          props: {
                            placement: "left",
                            transfer: true,
                            delay: 1000
                          }
                        },
                        [
                          h("Button", {
                            props: {
                              shape: "circle",
                              size: "small",
                              icon: "md-trash",
                              type: "error"
                            }
                          }),
                          h(
                            "p",
                            {
                              slot: "content",
                              style: {
                                whiteSpace: "normal"
                              }
                            },
                            "删除"
                          )
                        ]
                      )
                    ]
                  );
                },
                (h, params, vm) => {
                  return h(
                    "Tooltip",
                    {
                      props: {
                        placement: "left",
                        transfer: true,
                        delay: 1000
                      }
                    },
                    [
                      h("Button", {
                        props: {
                          shape: "circle",
                          size: "small",
                          icon: "md-create",
                          type: "primary"
                        },
                        on: {
                          click: () => {
                            vm.$emit("on-edit", params);
                            vm.$emit("input", params.tableData);
                          }
                        }
                      }),
                      h(
                        "p",
                        {
                          slot: "content",
                          style: {
                            whiteSpace: "normal"
                          }
                        },
                        "编辑"
                      )
                    ]
                  );
                }
              ]
            }
          ],
          data: []
        }
      },
      formSource: {
            DncBoiler : [] ,
      },
      styles: {
        height: "calc(100% - 55px)",
        overflow: "auto",
        paddingBottom: "53px",
        position: "static"
      }
    };
  },
  computed: {
    formTitle() {
      if (this.formModel.mode === "create") {
        return "创建";
      }
      if (this.formModel.mode === "edit") {
        return "编辑";
      }
      return "";
    },
    selectedRows() {
      return this.formModel.selection;
    },
    selectedRowsId() {
      return this.formModel.selection.map(x => x.id);
    }
  },
  methods: {
  
    loadKyqList() {
      let o=this;
      getKyqList(this.stores.Kyq.query).then(res => {
        this.stores.Kyq.data = getDateMore(res.data.data,1,[]);
        this.stores.Kyq.query.totalCount = res.data.totalCount;
      });

      getBoilerListAll().then(res => {
        let t=[];
        for(var i=0;i<res.data.data.length;i++){
            var item = res.data.data[i];
            t.push({ value: item.id , text: item.k_Name_kw, disabled: !item.status });
        }
        o.boilers=t;
      });

    },
    handleOpenFormWindow() {
      this.formModel.opened = true;
    },
    handleCloseFormWindow() {
      this.formModel.opened = false;
    },
    handleSwitchFormModeToCreate() {
      this.formModel.mode = "create";
    },
    handleSwitchFormModeToEdit() {
      this.formModel.mode = "edit";
      this.handleOpenFormWindow();
    },
    handleEdit(params) {
      this.handleSwitchFormModeToEdit();
      this.handleResetFormKyq();
      this.doLoadAllForinkey();
      this.doLoadKyq(params.row.id);
    },
    handleSelect(selection, row) {},
    handleSelectionChange(selection) {
      this.formModel.selection = selection;
    },
    handleRefresh() {
      this.loadKyqList();
    },
    handleShowCreateWindow() {
      this.handleSwitchFormModeToCreate();
      this.handleOpenFormWindow();
      this.handleResetFormKyq();
      this.doLoadAllForinkey();
      this.formModel.fields={
          K_Name_kw:"",
          LR:0,
          Way:0,
          DncBoiler_Name:"",
          status: 1,
          isDeleted: 0
      };
    },
    handleSubmitKyq() {
      let valid = this.validateKyqForm();
      let o=this;
      valid.then(function(data){ 
        if (data) {
          if (o.formModel.mode === "create") {
            o.doCreateKyq();
          }
          if (o.formModel.mode === "edit") {
            o.doEditKyq();
          }
        }
      });
    },
    handleResetFormKyq() {
      this.$refs["formKyq"].resetFields();
    },
    doCreateKyq() {
      
      createKyq(this.formModel.fields).then(res => {
        if (res.data.code === 200) {
          this.$Message.success(res.data.message);
          this.loadKyqList();
        } else {
          this.$Message.warning(res.data.message);
        }
        this.handleCloseFormWindow();
      });
    },
    doEditKyq() {
      editKyq(this.formModel.fields).then(res => {
        if (res.data.code === 200) {
          this.$Message.success(res.data.message);
          this.loadKyqList();
        } else {
          this.$Message.warning(res.data.message);
        }
        this.handleCloseFormWindow();
      });
    },
    validateKyqForm() {
      let _valid = false;
      let o=this;
      return new Promise(function(resolve, reject){
        o.$refs["formKyq"].validate(valid => {
          if (!valid) {
            o.$Message.error("请完善表单信息");
            _valid = false;
          } else {
            _valid = true;
          }
          resolve(_valid);
        });
      });
    },
    doLoadKyq(id) {
      loadKyq({ code: id+"" }).then(res => {
        var op=upperKey(res.data.data);
   //     op.DncBoiler_Name=parseInt(op.DncBoiler_Name);
        this.formModel.fields = op;
      });
    },
    doLoadAllForinkey() {
      let o=this;
      ////{PageSize:10,CurrentPage:1,Status:1,IsDeleted:0,Sort:[{Field:"id",Direct:"Desc"}],Kw:""}
      getBoilerListAll().then(res => {
        let t=[];
        for(var i=0;i<res.data.data.length;i++){
            var item = res.data.data[i];
            t.push({ value: item.id , text: item.k_Name_kw, disabled: !item.status });
        }
        o.formSource.DncBoiler=t;
      });
    },
    handleDelete(params) {
      this.doDelete(params.row.id);
    },
    doDelete(ids) {
      if (!ids) {
        this.$Message.warning("请选择至少一条数据");
        return;
      }
      deleteKyq(ids).then(res => {
        if (res.data.code === 200) {
          this.$Message.success(res.data.message);
          this.loadKyqList();
        } else {
          this.$Message.warning(res.data.message);
        }
      });
    },
    handleBatchCommand(command) {
      if (!this.selectedRowsId || this.selectedRowsId.length <= 0) {
        this.$Message.warning("请选择至少一条数据");
        return;
      }
      this.$Modal.confirm({
        title: "操作提示",
        content:
          "<p>确定要执行当前 [" +
          this.commands[command].title +
          "] 操作吗?</p>",
        loading: true,
        onOk: () => {
          this.doBatchCommand(command);
        }
      });
    },
    doBatchCommand(command) {
      batchCommand({
        command: command,
        ids: this.selectedRowsId.join(",")
      }).then(res => {
        if (res.data.code === 200) {
          this.$Message.success(res.data.message);
          this.loadKyqList();
          this.formModel.selection=[];
        } else {
          this.$Message.warning(res.data.message);
        }
        this.$Modal.remove();
      });
    },
    handleSearchKyq() {
      this.loadKyqList();
    },
    rowClsRender(row, index) {
      if (row.isDeleted) {
        return "table-row-disabled";
      }
      return "";
    },
    handlePageChanged(page) {
      this.stores.Kyq.query.currentPage = page;
      this.loadKyqList();
    },
    handlePageSizeChanged(pageSize) {
      this.stores.Kyq.query.pageSize = pageSize;
      this.loadKyqList();
    },
    handleInputData(){
      this.modal1=true;
    },
    handleSuccess () {
      this.validated = true
    },
    handleInput (dd) {
      if(dd[0]){
        let len=dd.length;
        let t=dd[len-1].length;
        if(this.$refs.m1n1.$children && t<=4){
          for (let index = 0; index < this.$refs.m1n1.$children.length; index++) {
            this.$refs.m1n1.$children[index].$el.className ="tdtd";
          }
          this.$refs.m1n1.$children[t-1].$el.className ="tdtd tdtdselect";
        }
      }
    },
    handleError (index) {
      this.validated = false
      this.errorIndex = index
    },
    handleShow () {
      if (!this.validated) {
        let row=this.errorIndex+1;
        this.$Modal.confirm({
                        title: '您的内容不规范',
                        content: '您的第 '+row+' 行 字段数不匹配，请修改'
              });
      } else {
        let puto = [];
        puto.push(this.dataorder);
        puto.push(this.pasteDataArr);
        let s= JSON.stringify(puto);
        batchCreateKyq({
          fsts: s
        }).then(res => {
            if (res.data.code === 200) {
              this.$Message.success(res.data.message);
              this.loadKyqList();
              this.modal1=false;
            } else {
              this.$Modal.confirm({
                        title: '您的内容不规范',
                        content: res.data.message
              });
            }
        });
      }
    },
    handleSortChange(column){
      this.stores.Kyq.query.sort= [
        {
          direct: column.order,
          field: column.key
        }
      ];
      this.loadKyqList();
    }
  },
  mounted() {
    this.loadKyqList();
  }
};
</script>






