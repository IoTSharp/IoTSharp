<template>
  <div class="register-container">
    <el-form ref="registerForm" :model="registerForm" :rules="loginRules" class="register-form" auto-complete="on" label-position="left">

      <div class="title-container">
        <h3 class="title">IoTSharp Register</h3>
      </div>
      <el-form-item prop="customerId">
        <span class="svg-container">
          <svg-icon icon-class="message" />
        </span>
        <el-input
          ref="customerId"
          v-model="registerForm.customerId"
          placeholder="Customer's Id"
          name="customerId"
          type="text"
          tabindex="1"
          auto-complete="off"
        />
      </el-form-item>
      <el-form-item prop="email">
        <span class="svg-container">
          <svg-icon icon-class="message" />
        </span>
        <el-input
          ref="email"
          v-model="registerForm.email"
          placeholder="Your email"
          name="email"
          type="text"
          tabindex="1"
          auto-complete="off"
        />
      </el-form-item>
      <el-form-item prop="phoneNumber">
        <span class="svg-container">
          <svg-icon icon-class="message" />
        </span>
        <el-input
          ref="phoneNumber"
          v-model="registerForm.phoneNumber"
          placeholder="PhoneNumber"
          name="phoneNumber"
          type="text"
          tabindex="1"
          auto-complete="off"
        />
      </el-form-item>
      <el-tooltip v-model="capsTooltip" content="Caps lock is On" placement="right" manual>
        <el-form-item prop="password">
          <span class="svg-container">
            <svg-icon icon-class="password" />
          </span>
          <el-input
            :key="passwordType"
            ref="password"
            v-model="registerForm.password"
            :type="passwordType"
            placeholder="Password"
            name="password"
            tabindex="2"
            auto-complete="on"
            @keyup.native="checkCapslock"
            @blur="capsTooltip = false"
            @keyup.enter.native="handleRegister"
          />
          <span class="show-pwd" @click="showPwd">
            <svg-icon :icon-class="passwordType === 'password' ? 'eye' : 'eye-open'" />
          </span>
        </el-form-item>
      </el-tooltip>
      <div style="position:relative" align="right">
        <div class="block">
          <el-button :loading="loading" type="primary" class="item-btn" size="medium" @click.native.prevent="handleRegister">Register</el-button>
          <el-button class="item-btn" type="primary" @click="showDialog=true">Getting a tenant?</el-button>
        </div>
        <div class="block">
          <el-button class="thirdparty-button" type="primary" @click="showDialog=true">Getting a tenant?</el-button>
        </div>
      </div>
    </el-form>
    <el-dialog title="Getting a tenant?" :visible.sync="showDialog">
      If you need to register a tenant,
      <br>
      you need to contact us.Our e-mail is mysticboy@live.com
      <br>
      或者加入QQ群 63631741
      <br>
      Here is a test customer ID, please copy it.

      <br>
      b1075b75-7170-437a-aa0a-9787d63ae1f8
      <br>
      <br>
      <!--<social-sign />-->
    </el-dialog>
  </div>
</template>

<script>
// import { validUsername } from '@/utils/validate'
import customerId from '@/utils/test-customer-id'
export default {
  name: 'Register',
  data() {
    const validatePassword = (rule, value, callback) => {
      if (value == null || value.length < 6 || value.trim().length === '') {
        callback(new Error('密码太短'))
      }
      const arrVerify = [
        { regName: 'Number', regValue: /^.*[0-9]+.*/ },
        { regName: 'LowerCase', regValue: /^.*[a-z]+.*/ },
        { regName: 'UpperCase', regValue: /^.*[A-Z]+.*/ },
        { regName: 'SpecialCharacters', regValue: /^.*[^a-zA-Z0-9]+.*/ }
      ]
      let regNum = 0 // 记录匹配的次数
      for (let iReg = 0; iReg < arrVerify.length; iReg++) {
        if (arrVerify[iReg].regValue.test(value)) {
          regNum = regNum + 1
        }
      }
      if (regNum < 3) {
        callback(new Error('密码应该包含数字字符和特殊符号'))
      } else {
        callback()
      }
    }
    return {
      registerForm: {
        email: '',
        phoneNumber: '',
        customerId: customerId,
        password: ''
      },
      loginRules: {
        password: [{ required: true, trigger: 'blur', validator: validatePassword }]
      },
      passwordType: 'password',
      capsTooltip: false,
      loading: false,
      showDialog: false,
      redirect: undefined,
      otherQuery: {}
    }
  },
  watch: {
    $route: {
      handler: function(route) {
        const query = route.query
        if (query) {
          this.redirect = query.redirect
          this.otherQuery = this.getOtherQuery(query)
        }
      },
      immediate: true
    }
  },
  created() {
    // window.addEventListener('storage', this.afterQRScan)
  },
  mounted() {
    if (this.registerForm.email === '') {
      this.$refs.email.focus()
    } else if (this.registerForm.username === '') {
      this.$refs.username.focus()
    } else if (this.registerForm.password === '') {
      this.$refs.password.focus()
    }
  },
  destroyed() {
    // window.removeEventListener('storage', this.afterQRScan)
  },
  methods: {
    checkCapslock({ shiftKey, key } = {}) {
      if (key && key.length === 1) {
        if (shiftKey && (key >= 'a' && key <= 'z') || !shiftKey && (key >= 'A' && key <= 'Z')) {
          this.capsTooltip = true
        } else {
          this.capsTooltip = false
        }
      }
      if (key === 'CapsLock' && this.capsTooltip === true) {
        this.capsTooltip = false
      }
    },
    showPwd() {
      if (this.passwordType === 'password') {
        this.passwordType = ''
      } else {
        this.passwordType = 'password'
      }
      this.$nextTick(() => {
        this.$refs.password.focus()
      })
    },
    handleRegister() {
      this.loading = true
      this.$refs.registerForm.validate(valid => {
        if (valid) {
          console.log('will call dispatch')
          console.log(this.registerForm)
          this.$store.dispatch('user/register', this.registerForm)
            .then(() => {
              this.$router.push({ path: '/login' })
              this.loading = false
            })
            .catch(() => {
              this.loading = false
            })
        } else {
          console.log('error submit!!')
          return false
        }
      })
    },
    getOtherQuery(query) {
      return Object.keys(query).reduce((acc, cur) => {
        if (cur !== 'redirect') {
          acc[cur] = query[cur]
        }
        return acc
      }, {})
    }
  }
}
</script>

<style lang="scss">
/* 修复input 背景不协调 和光标变色 */
/* Detail see https://github.com/PanJiaChen/vue-element-admin/pull/927 */

$bg:#283443;
$light_gray:#fff;
$cursor: #fff;

@supports (-webkit-mask: none) and (not (cater-color: $cursor)) {
  .register-container .el-input input {
    color: $cursor;
  }
}

/* reset element-ui css */
.register-container {
  .el-input {
    display: inline-block;
    height: 47px;
    width: 85%;

    input {
      background: transparent;
      border: 0px;
      -webkit-appearance: none;
      border-radius: 0px;
      padding: 12px 5px 12px 15px;
      color: $light_gray;
      height: 47px;
      caret-color: $cursor;

      &:-webkit-autofill {
        box-shadow: 0 0 0px 1000px $bg inset !important;
        -webkit-text-fill-color: $cursor !important;
      }
    }
  }

  .el-form-item {
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(0, 0, 0, 0.1);
    border-radius: 5px;
    color: #454545;
  }
}
</style>

<style lang="scss" scoped>
$bg:#2d3a4b;
$dark_gray:#889aa4;
$light_gray:#eee;

.register-container {
  min-height: 100%;
  width: 100%;
  background-color: $bg;
  overflow: hidden;

  .register-form {
    position: relative;
    width: 520px;
    max-width: 100%;
    padding: 160px 35px 0;
    margin: 0 auto;
    overflow: hidden;
  }

  .tips {
    font-size: 14px;
    color: #fff;
    margin-bottom: 10px;

    span {
      &:first-of-type {
        margin-right: 16px;
      }
    }
  }

  .svg-container {
    padding: 6px 5px 6px 15px;
    color: $dark_gray;
    vertical-align: middle;
    width: 30px;
    display: inline-block;
  }

  .title-container {
    position: relative;

    .title {
      font-size: 26px;
      color: $light_gray;
      margin: 0px auto 40px auto;
      text-align: center;
      font-weight: bold;
    }
  }

  .show-pwd {
    position: absolute;
    right: 10px;
    top: 7px;
    font-size: 16px;
    color: $dark_gray;
    cursor: pointer;
    user-select: none;
  }

  .thirdparty-button {
    position: absolute;
    right: 0;
    bottom: 6px;
  }

  @media only screen and (max-width: 470px) {
    .thirdparty-button {
      display: none;
    }
  }
}
</style>
