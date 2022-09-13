"use strict";(self.webpackChunkiotsharp=self.webpackChunkiotsharp||[]).push([[598],{3905:function(e,t,r){r.d(t,{Zo:function(){return c},kt:function(){return d}});var n=r(7294);function a(e,t,r){return t in e?Object.defineProperty(e,t,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[t]=r,e}function o(e,t){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),r.push.apply(r,n)}return r}function i(e){for(var t=1;t<arguments.length;t++){var r=null!=arguments[t]?arguments[t]:{};t%2?o(Object(r),!0).forEach((function(t){a(e,t,r[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):o(Object(r)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))}))}return e}function p(e,t){if(null==e)return{};var r,n,a=function(e,t){if(null==e)return{};var r,n,a={},o=Object.keys(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||(a[r]=e[r]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(a[r]=e[r])}return a}var l=n.createContext({}),s=function(e){var t=n.useContext(l),r=t;return e&&(r="function"==typeof e?e(t):i(i({},t),e)),r},c=function(e){var t=s(e.components);return n.createElement(l.Provider,{value:t},e.children)},u={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},h=n.forwardRef((function(e,t){var r=e.components,a=e.mdxType,o=e.originalType,l=e.parentName,c=p(e,["components","mdxType","originalType","parentName"]),h=s(r),d=a,m=h["".concat(l,".").concat(d)]||h[d]||u[d]||o;return r?n.createElement(m,i(i({ref:t},c),{},{components:r})):n.createElement(m,i({ref:t},c))}));function d(e,t){var r=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var o=r.length,i=new Array(o);i[0]=h;var p={};for(var l in t)hasOwnProperty.call(t,l)&&(p[l]=t[l]);p.originalType=e,p.mdxType="string"==typeof e?e:a,i[1]=p;for(var s=2;s<o;s++)i[s]=r[s];return n.createElement.apply(null,i)}return n.createElement.apply(null,r)}h.displayName="MDXCreateElement"},5470:function(e,t,r){r.r(t),r.d(t,{assets:function(){return c},contentTitle:function(){return l},default:function(){return d},frontMatter:function(){return p},metadata:function(){return s},toc:function(){return u}});var n=r(3117),a=r(102),o=(r(7294),r(3905)),i=["components"],p={sidebar_position:4},l="\u5728Linux\u4e2d\u90e8\u7f72IoTSharp",s={unversionedId:"tutorial-basics/deploy_linux",id:"tutorial-basics/deploy_linux",title:"\u5728Linux\u4e2d\u90e8\u7f72IoTSharp",description:"\u672c\u6559\u7a0b\u4f7f\u7528Sqlite \u4e3a\u6570\u636e\u5b58\u50a8 \u65b9\u5f0f\u8fdb\u884c\u90e8\u7f72",source:"@site/docs/tutorial-basics/deploy_linux.md",sourceDirName:"tutorial-basics",slug:"/tutorial-basics/deploy_linux",permalink:"/docs/tutorial-basics/deploy_linux",editUrl:"https://github.com/IoTSharp/IoTSharp/edit/master/docs/docs/tutorial-basics/deploy_linux.md",tags:[],version:"current",sidebarPosition:4,frontMatter:{sidebar_position:4},sidebar:"tutorialSidebar",previous:{title:"\u4f7f\u7528Dcoker\u90e8\u7f72",permalink:"/docs/tutorial-basics/deploy_by_docker"},next:{title:"\u5728Windows\u4e2d\u90e8\u7f72IoTSharp",permalink:"/docs/tutorial-basics/deploy_win"}},c={},u=[{value:"\u6ce8\u518c",id:"\u6ce8\u518c",level:2},{value:"\u8bbf\u95ee",id:"\u8bbf\u95ee",level:2}],h={toc:u};function d(e){var t=e.components,p=(0,a.Z)(e,i);return(0,o.kt)("wrapper",(0,n.Z)({},h,p,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"\u5728linux\u4e2d\u90e8\u7f72iotsharp"},"\u5728Linux\u4e2d\u90e8\u7f72IoTSharp"),(0,o.kt)("p",null,"\u672c\u6559\u7a0b\u4f7f\u7528Sqlite \u4e3a\u6570\u636e\u5b58\u50a8 \u65b9\u5f0f\u8fdb\u884c\u90e8\u7f72"),(0,o.kt)("h1",{id:"\u4e0b\u8f7d"},"\u4e0b\u8f7d"),(0,o.kt)("p",null,"\u9996\u5148\u5728 ",(0,o.kt)("a",{parentName:"p",href:"https://github.com/IoTSharp/IoTSharp/releases"},"https://github.com/IoTSharp/IoTSharp/releases")," \u6216\u8005 ",(0,o.kt)("a",{parentName:"p",href:"https://gitee.com/IoTSharp/IoTSharp/releases"},"https://gitee.com/IoTSharp/IoTSharp/releases")," \u4e2d\u4e0b\u8f7d\u6700\u65b0\u7248\u672c\u7684\u5b89\u88c5\u5305\uff0c \u5e38\u7528\u7cfb\u7edf\u4e2d\u538b\u7f29\u5305\u9009\u62e9 ",(0,o.kt)("a",{parentName:"p",href:"https://github.com/IoTSharp/IoTSharp/releases/download/v2.8/IoTSharp.Release.linux-x64.zip"},"IoTSharp.Release.linux-x64.zip"),"  , \u5982\u679c\u662f\u6811\u8393\u6d3e\u7248\u672c\u5219\u4e0b\u8f7d ",(0,o.kt)("a",{parentName:"p",href:"https://github.com/IoTSharp/IoTSharp/releases/download/v2.8/IoTSharp.Release.linux-arm64.zip"},"IoTSharp.Release.linux-arm64.zip"),"   \u81f3\u672c\u5730\u3002 "),(0,o.kt)("h1",{id:"\u76f4\u63a5\u542f\u52a8"},"\u76f4\u63a5\u542f\u52a8"),(0,o.kt)("p",null,"\u89e3\u538b\u538b\u7f29\u5305\u540e\uff0c \u6211\u4eec\u53ef\u4ee5\u770b\u5230\u91cc\u9762 \u6709\u4e00\u4e2a IoTSharp \u6587\u4ef6\uff0c \u4f7f\u7528 chmod 777 IoTSharp \uff0c \u7136\u540e \u5728\u547d\u4ee4\u884c\u4f7f\u7528 ./IoTSharp \u5373\u53ef\u3002 \u542f\u52a8\u540e\uff0c \u5373\u53ef\u5728\u6d4f\u89c8\u5668\u4e2d\u6253\u5f00 http://localhost:2927 \u6765\u8bbf\u95ee\u3002 "),(0,o.kt)("h1",{id:"\u6ce8\u518c\u4e3a\u670d\u52a1"},"\u6ce8\u518c\u4e3a\u670d\u52a1"),(0,o.kt)("p",null,"IoTSharp \u5df2\u7ecf\u652f\u6301\u4e86Linux \u670d\u52a1\u7684\u65b9\u5f0f\u8fd0\u884c\uff0c \u6309\u7167\u4e0b\u9762\u7684\u6b65\u9aa4\u53ef\u4ee5\u5c06IoTSharp \u6ce8\u518c\u4e3aLInux daemon  "),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},"mkdir  /var/lib/iotsharp/   # \u521b\u5efa\u8fd0\u884c\u76ee\u5f55 "),(0,o.kt)("li",{parentName:"ul"},"cp ./*  /var/lib/iotsharp/   # \u5c06\u6240\u6709\u6587\u4ef6\u62f7\u8d1d\u81f3\u76ee\u6807\u76ee\u5f55"),(0,o.kt)("li",{parentName:"ul"},"chmod 777 /var/lib/iotsharp/IoTSharp  # \u8bbe\u7f6eIoTSharp \u7684\u53ef\u6267\u884c\u6743\u9650"),(0,o.kt)("li",{parentName:"ul"},"cp  iotsharp.service   /etc/systemd/system/iotsharp.service  # \u5c06\u670d\u52a1\u6587\u4ef6\u62f7\u8d1d\u81f3\u7cfb\u7edf"),(0,o.kt)("li",{parentName:"ul"},"sudo systemctl enable  /etc/systemd/system/iotsharp.service   # \u542f\u7528\u670d\u52a1"),(0,o.kt)("li",{parentName:"ul"},"sudo systemctl start  iotsharp.service   # \u542f\u52a8\u6b64\u670d\u52a1"),(0,o.kt)("li",{parentName:"ul"},"sudo journalctl -fu  iotsharp.service   # \u67e5\u770b\u8be5\u670d\u52a1\u65e5\u5fd7 ")),(0,o.kt)("h2",{id:"\u6ce8\u518c"},"\u6ce8\u518c"),(0,o.kt)("p",null,"Chrome\u6d4f\u89c8\u5668\u8bbf\u95ee ",(0,o.kt)("inlineCode",{parentName:"p"},"http://localhost:2927/")),(0,o.kt)("p",null,(0,o.kt)("img",{alt:"\u6ce8\u518c",src:r(4652).Z,width:"888",height:"840"})),(0,o.kt)("h2",{id:"\u8bbf\u95ee"},"\u8bbf\u95ee"),(0,o.kt)("p",null,"\u6ce8\u518c\u540e\u767b\u5165\u8fdb\u5165\u9996\u9875\n",(0,o.kt)("img",{alt:"\u8bbf\u95ee",src:r(1011).Z,width:"1902",height:"831"})))}d.isMDXComponent=!0},1011:function(e,t,r){t.Z=r.p+"assets/images/iotsharp-dashboard-e2f47226cba57d08531957d496c86b97.png"},4652:function(e,t,r){t.Z=r.p+"assets/images/iotsharp-regeist-aab9025fdcae4e6347e17a575cfcac26.png"}}]);