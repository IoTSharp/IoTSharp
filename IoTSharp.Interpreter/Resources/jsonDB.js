/**
 +-----------------------------------------
 * jsonDB 	基于json数据格式构建的数据模型
 +-----------------------------------------
 * @description		对json数据检索，删除，查询和更新
 * @author	戚银(web程序猿)	thinkercode.china@gamil.com
 * @date 2014年6月28日
 * @version	0.1
 * @blog http://blog.csdn.net/thinkercode/
 +-----------------------------------------
 * 简介：
 * jsonDB是js的一个类库，是基于json数据格式构建的数据
 * 模型实现对json数据增删改查。jsonDB的构建源自于HTML5
 * 本地存储的一个应用需求，可以通过sql对json数据进行增
 * 删改查，同时该类库提供强大的where检索条件，数据排序，
 * limit查询条件限制等数据库基本功能。通过jsonDB可以轻
 * 松维护一个库/表或多个库/表，而无需额外实现json的数据
 * 的维护等，在该类库完善以后为简化sql操作，基于jsonDB核
 * 心模块扩展了连贯操作模型，简化对jsonDB的操作以及sql语
 * 句出错的概率。
 +-----------------------------------------
 * 当前版本的不足：
 * 1.无法支持查询字段运算
 * 2.不支持对没有选取的查询字段排序
 * 3.只支持单个字段排序，无法进行组合排序操作
 * 4.update、delete语句不支持order by子句，导致limit子句功能弱化
 * 5.编写where条件是必须使用()包含where条件
 * 6.无法选取深层次的字段作为返回字段
 * 7.没有错误或异常解决方案
 * 8.不支持外部扩展
 +-----------------------------------------
 * jsonDB的使用：
 * jsonDB会提供一个详细的使用手册，其中包含大量示例
 * jsonDB还有很多可以使用的技巧并没有一一包含在手册中
 * 期望强大的网友一起挖掘jsonDB的潜在使用技巧，并分享给大家
 * 如果使用中发现问题可以随时发送邮件，或者通过博客留言等方式一起探讨
 * 同时鉴于自己实力有限，期望发动网友一起扩展jsonDB，
 * 能在未来前端应用中奉献一份力量
 +-----------------------------------------
 */


_history = null,
    DBCore = {},
    Database = {},
    DBExpand = {};


/**
 * [jsonDB 初始化模型，支持定义本地数据库和数据表]
 * @param  mixed data 	数据
 * @param  string dbName	数据库名字
 * @return jsonDB
 */
var jsonDB = function (data, dbName) {

    //创建数据库或者数据表
    if (data) {
        dbName = dbName || 'json_db';
        eval('Database.' + dbName + '= data');
    }

    return jsonDB.fn.init();
}

jsonDB.fn = jsonDB.prototype = {
    //初始化插件
    init: function (alias) {
        if (alias) {

        }
        return this;
    },

    query: function (sql) {
        var type = sql.match(/^(\w+)/);
        switch (type[0]) {
            case 'select':
            case 'delete':
            case 'update':
                return eval('DBCore.fn.' + type[0] + '(sql+" ")');
                break;
            default:
                return false;
                break;
        }
    },

    insert: function (data, dbName) {
        if (data) {
            dbName = dbName || 'json_db';
            eval('Database.' + dbName + '.push(data)');
        }
        return this;
    },

    findAll: function (dbName) {
        if (dbName) {
            return eval('Database.' + dbName);
        }
        return Database;
    }
};

/**
 * [DBExpand 数据库核心功能扩展]
 */
DBExpand = DBExpand.prototype = {
    sqlParam: {
        fields: '*',
        table: 'json_db',
        where: null,
        order: null,
        limit: null,
    },

    add: function (data) {
        return this.insert(data, this.sqlParam.table);
    },

    select: function () {
        var sql = 'select ' + this.sqlParam.fields + ' from ' + this.sqlParam.table;
        if (this.sqlParam.where) {
            sql += ' where ' + this.sqlParam.where;
        }
        if (this.sqlParam.order) {
            sql += ' order by ' + this.sqlParam.order;
        }
        if (this.sqlParam.limit) {
            sql += ' limit ' + this.sqlParam.limit;
        }

        this.clear();
        return this.query(sql);
    },

    update: function (data) {
        if (data.length < 1) {
            return false;
        }

        var sql = 'update ' + this.sqlParam.table + ' set ' + data;
        if (this.sqlParam.where) {
            sql += ' where ' + this.sqlParam.where;
        }
        if (this.sqlParam.limit) {
            sql += ' limit ' + this.sqlParam.limit;
        }
        this.clear();
        return this.query(sql);
    },

    delete: function () {
        if (this.sqlParam.where.length < 1) {
            return false;
        }

        var sql = 'delete from ' + this.sqlParam.table;
        if (this.sqlParam.where) {
            sql += ' where ' + this.sqlParam.where;
        }
        if (this.sqlParam.limit) {
            sql += ' limit ' + this.sqlParam.limit;
        }
        this.clear();
        return this.query(sql);
    },

    drop: function (dbName) {
        //创建数据库或者数据表
        if (data) {
            dbName = dbName || 'json_db';
            eval('Database.' + dbName + '= null');
        }
        return this;
    },

    field: function (fields) {
        if (typeof fields == 'object') {
            this.sqlParam.fields = fields.join(',');
        } else {
            this.sqlParam.fields = fields;
        }
        return this;
    },

    table: function (table) {
        this.sqlParam.table = table;
        return this;
    },

    where: function (where) {
        this.sqlParam.where = '(' + where + ')';
        return this;
    },

    order: function (order) {
        this.sqlParam.order = order;
        return this;
    },

    limit: function (limit) {
        this.sqlParam.limit = limit;
        return this;
    },

    clear: function () {
        this.sqlParam.fields = '*';
        this.sqlParam.where = null;
        this.sqlParam.order = null;
        this.sqlParam.limit = null;
    }
}

/**
 * [DBCore 数据库核心]
 */
DBCore.fn = DBCore.prototype = {
    SqlRegExp: {
        fields: '([a-z0-9_\\,\\.\\s\\*]*?\\s+)',
        from: '(from\\s+([a-z0-9_\\.]+)\\s*)?',
        where: '(?:where\\s+(\\(.*\\))\\s*)?',
        order: '(?:order\\s+by\\s+([a-z0-9_\\,\\.]+))?\\s+(asc|desc|ascnum|descnum)?\\s*',
        limit: '(?:limit\\s+([0-9\\,]+))?',
        set: '(set\\s+(.*?)\\s+)',
        table: '(([a-z0-9_\\.]*?)\\s*)?',
    },

    select: function (sql) {
        var params = { fields: ["*"], from: "json_db", where: "", orderby: [], order: "asc", limit: [] },
            SqlRegExp = this.SqlRegExp,
            reg = '^(select)\\s+' + SqlRegExp.fields + SqlRegExp.from + SqlRegExp.where + SqlRegExp.order + SqlRegExp.limit,
            sqlReg = new RegExp(reg, 'i'),
            sqlFields = sql.match(sqlReg),
            options = {
                fields: sqlFields[2].replace(' ', '').split(','),
                from: (sqlFields[4] == undefined) ? 'json_db' : sqlFields[4],
                where: (sqlFields[5] == undefined) ? "true" : sqlFields[5].replace(/([^\>\<\!\=])=([^\>\<\!\=])/gi, '$1 == $2').replace(/\sand\s/gi, ' && ').replace(/\sor\s/gi, ' || ').replace(/`/gi, ''),
                orderby: (sqlFields[6] == undefined) ? [] : sqlFields[6].replace(' ', '').split(','),
                order: (sqlFields[7] == undefined) ? "asc" : sqlFields[7],
                limit: (sqlFields[8] == undefined) ? [] : sqlFields[8].replace(' ', '').split(',')
            };

        for (i in options) {
            params[i] = options[i];
        }

        var result = [];
        result = this.filter(params, function (data) {
            if (options.fields.length == 0 || options.fields[0] == "*") {
                return data;
            }

            var result = {};
            for (var i in options.fields) {
                result[options.fields[i]] = data[options.fields[i]];
            }
            return result;
        });
        result = this.orderBy(result, options);
        result = this.limit(result, options);
        return result;
    },

    update: function (sql) {
        var params = { from: "json_db", where: "", limit: [], set: [] },
            SqlRegExp = this.SqlRegExp,
            reg = '^(update)\\s+' + SqlRegExp.table + SqlRegExp.set + SqlRegExp.where + SqlRegExp.limit,
            sqlReg = new RegExp(reg, 'i'),
            sqlFields = sql.match(sqlReg),
            options = {
                from: (sqlFields[3] == undefined) ? 'json_db' : sqlFields[3],
                set: (sqlFields[5] == undefined) ? [] : sqlFields[5].replace(' ', '').split(','),
                where: (sqlFields[6] == undefined) ? "true" : sqlFields[6].replace(/([^\>\<\!\=])=([^\>\<\!\=])/gi, '$1 == $2').replace(/\sand\s/gi, ' && ').replace(/\sor\s/gi, ' || ').replace(/`/gi, ''),
                limit: (sqlFields[7] == undefined) ? [] : sqlFields[7].replace(' ', '').split(',')
            };

        for (i in options) {
            params[i] = options[i];
        }
        var jsonData = eval('Database.' + options.from),
            tally = 0,
            extent = this.extent(params),
            setLen = options.set.length,
            affected_rows = 0;

        if (setLen < 1) {
            return false;
        }

        options.where = options.where || "true";
        for (var i in jsonData) {
            with (jsonData[i]) {
                if (eval(options.where)) {
                    if (affected_rows >= extent.start && tally < extent.stop) {
                        for (var j = 0; j < setLen; ++j) {
                            eval(options.set[j]);
                        }
                        ++tally;
                    } else if (tally == extent.stop) {
                        return tally;
                    }
                    ++affected_rows;
                }
            }
        }
        return tally;
    },

    delete: function (sql) {
        var params = { from: "json_db", where: "", limit: [] },
            SqlRegExp = this.SqlRegExp,
            reg = '^(delete)\\s+' + SqlRegExp.from + SqlRegExp.where + SqlRegExp.limit,
            sqlReg = new RegExp(reg, 'i'),
            sqlFields = sql.match(sqlReg),
            options = {
                from: (sqlFields[3] == undefined) ? 'json_db' : sqlFields[3],
                where: (sqlFields[4] == undefined) ? "true" : sqlFields[4].replace(/([^\>\<\!\=])=([^\>\<\!\=])/gi, '$1 == $2').replace(/\sand\s/gi, ' && ').replace(/\sor\s/gi, ' || ').replace(/`/gi, ''),
                limit: (sqlFields[5] == undefined) ? [] : sqlFields[5].replace(' ', '').split(',')
            };

        for (i in options) {
            params[i] = options[i];
        }

        var jsonData = eval('Database.' + options.from + '.concat()'),
            tally = 0,
            extent = this.extent(params),
            affected_rows = 0;

        options.where = options.where || "true";
        for (var i in jsonData) {
            with (jsonData[i]) {
                if (eval(options.where)) {
                    if (affected_rows >= extent.start && tally < extent.stop) {
                        eval('Database.' + options.from + '.splice(i-tally,1)');
                        ++tally;
                    } else if (tally == extent.stop) {
                        return tally;
                    }
                    ++affected_rows;
                }
            }
        }
        return tally;
    },

    filter: function (options, callback) {
        var jsonData = eval('Database.' + options.from),
            result = [],
            index = 0;

        options.where = options.where || "true";
        for (var i in jsonData) {
            with (jsonData[i]) {
                if (eval(options.where)) {
                    if (callback) {
                        result[index++] = callback(jsonData[i]);
                    } else {
                        result[index++] = jsonData[i];
                    }
                }
            }
        }

        return result;
    },

    orderBy: function (result, options) {
        if (options.orderby.length == 0) {
            return result;
        }

        result.sort(function (a, b) {
            switch (options.order.toLowerCase()) {
                case "desc": return (eval('a.' + options.orderby[0] + ' < b.' + options.orderby[0])) ? 1 : -1;
                case "asc": return (eval('a.' + options.orderby[0] + ' > b.' + options.orderby[0])) ? 1 : -1;
                case "descnum": return (eval('a.' + options.orderby[0] + ' - b.' + options.orderby[0]));
                case "ascnum": return (eval('b.' + options.orderby[0] + ' - a.' + options.orderby[0]));
            }
        });

        return result;
    },

    limit: function (result, options) {
        switch (options.limit.length) {
            case 0:
                return result;
            case 1:
                return result.splice(0, options.limit[0]);
            case 2:
                return result.splice(options.limit[0], options.limit[1]);
        }
    },

    extent: function (options) {
        switch (options.limit.length) {
            case 0:
                return { start: 0, stop: 9e+99 };
            case 1:
                return { start: 0, stop: options.limit[0] };
            case 2:
                return { start: options.limit[0], stop: options.limit[1] };
        }
    }
}



//追加扩展功能
jsonDB.fn = jsonDB.prototype = extend(jsonDB.fn, DBExpand);

//合并对象方法
function extend() {
    var paramsLen = arguments.length;
    if (paramsLen < 1) {
        return false;
    }

    var target = arguments[0];
    for (var i = 1; i < paramsLen; ++i) {
        for (var j in arguments[i]) {
            target[j] = arguments[i][j];
        }
    }

    return target;
}
 