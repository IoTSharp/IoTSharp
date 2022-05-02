import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const FeatureList = [
  {
    title: '数字孪生',
    Svg: require('../../static/img/undraw_docusaurus_mountain.svg').default,
    description: (
      <>
        IoTSharp 通过服务侧、客户端侧属性、遥测数据、RPC等实现了数字孪生， 实现了所有设备统一接口提供给业务调用。
      </>
    ),
  },
  {
    title: '规则链',
    Svg: require('../../static/img/undraw_docusaurus_tree.svg').default,
    description: (
      <>
         IoTSharp 允许你通过JS、C#、Lua、Python、SQL等脚本处理数据并结合动态Linq表达式处理节点与节点之间的走向以实现数据清洗、告警、事件等相应手段， 也可以将网关数据通过规则链合成真正的数字孪生设备。 
      </>
    ),
  },
  {
    title: '多样性',
    Svg: require('../../static/img/undraw_docusaurus_react.svg').default,
    description: (
      <>
        IoTSharp 重复考虑到物联网架构的重要性，因此我们具有丰富的中间件支持，比如消息中间件(RabbitMQ、RabbitMQ、Kafka、ZeroMQ)进行处理、也支持多种时序数据库（InfluxDB、Taos、TimescaleDB）存储和处理遥测数据。基础数据我们支持多种关系型数据库(MySQl、PostgreSQL、Oracle、Sql Server、Sqlite)
      </>
    ),
  },
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} alt={title} />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
